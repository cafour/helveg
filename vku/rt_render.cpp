#include "rt_render.hpp"

#include <glm/gtc/matrix_transform.hpp>
#include <vector>

/*
 * Based on the VK_KHR_ray_tracing tutorial by NVIDIA:
 * https://nvpro-samples.github.io/vk_raytracing_tutorial_KHR/
 */

const std::vector<const char *> requiredDevicesExtensions = std::vector<const char *> {
    VK_KHR_SWAPCHAIN_EXTENSION_NAME,
    VK_KHR_RAY_TRACING_EXTENSION_NAME,
    VK_KHR_MAINTENANCE3_EXTENSION_NAME,
    VK_KHR_PIPELINE_LIBRARY_EXTENSION_NAME,
    VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME,
    VK_KHR_BUFFER_DEVICE_ADDRESS_EXTENSION_NAME
};

static VkPhysicalDeviceVulkan12Features getDeviceFeatures()
{
    VkPhysicalDeviceVulkan12Features features = {};
    features.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_2_FEATURES;
    features.bufferDeviceAddress = VK_TRUE;
    return features;
}

VkPhysicalDeviceVulkan12Features deviceFeatures = getDeviceFeatures();

VkPhysicalDeviceRayTracingFeaturesKHR rtFeatures = {
    VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_TRACING_FEATURES_KHR,
    &deviceFeatures
};

vku::RTRender::RTRender(int width, int height, World world, const std::string &title, bool debug)
    : _instanceCore("RTRender", true, debug)
    , _displayCore(_instanceCore.instance(), width, height, title, requiredDevicesExtensions, nullptr, &rtFeatures)
    , _swapchainCore(_displayCore)
    , _renderCore(
          _displayCore,
          _swapchainCore,
          [this](auto &f) { return createFramebuffer(f); },
          [this](auto cb, auto &f) { recordCommandBuffer(cb, f); })
    , _transferCore(_displayCore.physicalDevice(), _displayCore.device())
    , _rtProperties(vku::findProperties<VkPhysicalDeviceRayTracingPropertiesKHR>(
          _displayCore.physicalDevice(),
          VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_TRACING_PROPERTIES_KHR))
    , _world(world)
{
    auto maybeMesh = vku::MeshCore::fromChunk(_transferCore, _world.chunks[0], true);
    auto geometryInfo = createGeometry(maybeMesh.value());
    GeometriesInfo geometries;
    geometries.createInfos.push_back(geometryInfo.createInfo);
    geometries.geometries.push_back(geometryInfo.geometry);
    geometries.offsets.push_back(geometryInfo.offset);
    auto blas = createBlas(geometries);
    auto asInstance = createASInstance(blas, glm::translate(glm::mat4(1), _world.chunkOffsets[0]), 0);
    std::vector instances { asInstance };
    auto tlas = createTlas(instances);
}

void vku::RTRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
}

vku::Framebuffer vku::RTRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    return {};
}

// returns the scratch and object size of an accelaration structure
static VkDeviceSize getASSize(
    VkDevice device,
    VkAccelerationStructureKHR as,
    VkAccelerationStructureMemoryRequirementsTypeKHR sizeType)
{
    VkAccelerationStructureMemoryRequirementsInfoKHR asReqs = {};
    asReqs.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_MEMORY_REQUIREMENTS_INFO_KHR;
    asReqs.type = sizeType;
    asReqs.accelerationStructure = as;
    asReqs.buildType = VK_ACCELERATION_STRUCTURE_BUILD_TYPE_DEVICE_KHR;

    VkMemoryRequirements2 memReqs = {};
    memReqs.sType = VK_STRUCTURE_TYPE_MEMORY_REQUIREMENTS_2;

    vkGetAccelerationStructureMemoryRequirementsKHR(device, &asReqs, &memReqs);
    return memReqs.memoryRequirements.size;
}

static vku::BackedBuffer getRTBuffer(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkDeviceSize size)
{
    auto buffer = vku::Buffer::exclusive(
        device,
        size,
        VK_BUFFER_USAGE_RAY_TRACING_BIT_KHR
            | VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT
            | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    auto memory = vku::DeviceMemory::deviceLocalBuffer(
        physicalDevice,
        device,
        buffer,
        VK_MEMORY_ALLOCATE_DEVICE_ADDRESS_BIT);
    return { std::move(buffer), std::move(memory) };
}

static vku::BackedBuffer getScratch(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkAccelerationStructureKHR as)
{
    auto scratchSize = getASSize(
        device,
        as,
        VK_ACCELERATION_STRUCTURE_MEMORY_REQUIREMENTS_TYPE_BUILD_SCRATCH_KHR);
    return getRTBuffer(physicalDevice, device, scratchSize);
}

vku::GeometryInfo vku::RTRender::createGeometry(vku::MeshCore &mesh)
{
    // prepare a single geometry object

    VkAccelerationStructureCreateGeometryTypeInfoKHR geometryCreate = {};
    geometryCreate.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_GEOMETRY_TYPE_INFO_KHR;
    geometryCreate.geometryType = VK_GEOMETRY_TYPE_TRIANGLES_KHR;
    geometryCreate.indexType = VK_INDEX_TYPE_UINT32;
    geometryCreate.vertexFormat = VK_FORMAT_R32G32B32_SFLOAT;
    geometryCreate.maxPrimitiveCount = static_cast<uint32_t>(mesh.indexCount() / 3);
    geometryCreate.maxVertexCount = static_cast<uint32_t>(mesh.vertexCount());
    geometryCreate.allowsTransforms = VK_FALSE;

    VkAccelerationStructureGeometryTrianglesDataKHR triangles = {};
    triangles.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_TRIANGLES_DATA_KHR;
    triangles.vertexFormat = geometryCreate.vertexFormat;
    triangles.vertexData.deviceAddress = vku::getBufferAddress(_displayCore.device(), mesh.vertexBuffer());
    triangles.vertexStride = sizeof(glm::vec3);
    triangles.indexType = geometryCreate.indexType;
    triangles.indexData.deviceAddress = vku::getBufferAddress(_displayCore.device(), mesh.indexBuffer());

    VkAccelerationStructureGeometryKHR geometry = {};
    geometry.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR;
    geometry.flags = VK_GEOMETRY_OPAQUE_BIT_KHR;
    geometry.geometry.triangles = triangles;
    geometry.geometryType = VK_GEOMETRY_TYPE_TRIANGLES_KHR;

    VkAccelerationStructureBuildOffsetInfoKHR offset = {};
    offset.firstVertex = 0;
    offset.primitiveCount = geometryCreate.maxPrimitiveCount;
    offset.primitiveOffset = 0;
    offset.transformOffset = 0;

    return { geometryCreate, geometry, offset };
}

vku::AccelerationStructure vku::RTRender::createBlas(vku::GeometriesInfo &geometries)
{
    VkAccelerationStructureCreateInfoKHR blasCreate = {};
    blasCreate.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR;
    blasCreate.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
    blasCreate.flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
    blasCreate.maxGeometryCount = static_cast<uint32_t>(geometries.createInfos.size());
    blasCreate.pGeometryInfos = geometries.createInfos.data();
    auto blas = vku::AccelerationStructure(_displayCore.device(), blasCreate);

    auto scratch = getScratch(_displayCore.physicalDevice(), _displayCore.device(), blas);
    VkDeviceAddress scratchAddress = vku::getBufferAddress(_displayCore.device(), scratch.buffer);

    auto commandBuffers = vku::CommandBuffers::beginSingle(_displayCore.device(), _renderCore.commandPool());
    auto cmd = commandBuffers.front();
    VkAccelerationStructureGeometryKHR *pGeometries = geometries.geometries.data();
    VkAccelerationStructureBuildGeometryInfoKHR buildInfo = {};
    buildInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR;
    buildInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
    buildInfo.flags = blasCreate.flags;
    buildInfo.update = VK_FALSE;
    buildInfo.srcAccelerationStructure = VK_NULL_HANDLE;
    buildInfo.dstAccelerationStructure = blas;
    buildInfo.geometryArrayOfPointers = VK_FALSE;
    buildInfo.geometryCount = static_cast<uint32_t>(geometries.geometries.size());
    buildInfo.ppGeometries = &pGeometries;
    buildInfo.scratchData.deviceAddress = scratchAddress;

    std::vector<const VkAccelerationStructureBuildOffsetInfoKHR *> pOffsets(geometries.offsets.size());
    for (size_t i = 0; i < geometries.offsets.size(); ++i) {
        pOffsets[i] = &geometries.offsets[i];
    }
    vkCmdBuildAccelerationStructureKHR(cmd, 1, &buildInfo, pOffsets.data());
    vkEndCommandBuffer(cmd);
    vku::CommandBuffers::submitSingle(commandBuffers, _displayCore.queue());

    return blas;
}

VkAccelerationStructureInstanceKHR vku::RTRender::createASInstance(
    VkAccelerationStructureKHR as,
    glm::mat4 transform,
    uint32_t instanceId)
{
    VkAccelerationStructureInstanceKHR instance = {};
    std::copy(&transform[0][0], &transform[3][0], &instance.transform.matrix[0][0]);
    instance.instanceCustomIndex = instanceId;
    instance.mask = 0xff;
    instance.instanceShaderBindingTableRecordOffset = 0;
    instance.flags = VK_GEOMETRY_INSTANCE_TRIANGLE_FACING_CULL_DISABLE_BIT_KHR;

    VkAccelerationStructureDeviceAddressInfoKHR addressInfo = {};
    addressInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_DEVICE_ADDRESS_INFO_KHR;
    addressInfo.accelerationStructure = as;

    instance.accelerationStructureReference = vkGetAccelerationStructureDeviceAddressKHR(
        _displayCore.device(),
        &addressInfo);
    return instance;
}

vku::AccelerationStructure vku::RTRender::createTlas(std::vector<VkAccelerationStructureInstanceKHR> &instances)
{
    VkAccelerationStructureCreateGeometryTypeInfoKHR geometryCreate = {};
    geometryCreate.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_GEOMETRY_TYPE_INFO_KHR;
    geometryCreate.geometryType = VK_GEOMETRY_TYPE_INSTANCES_KHR;
    geometryCreate.maxPrimitiveCount = static_cast<uint32_t>(instances.size());
    geometryCreate.allowsTransforms = VK_TRUE;

    VkAccelerationStructureCreateInfoKHR tlasCreate = {};
    tlasCreate.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR;
    tlasCreate.type = VK_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL_KHR;
    tlasCreate.flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
    tlasCreate.maxGeometryCount = 1;
    tlasCreate.pGeometryInfos = &geometryCreate;
    auto tlas = vku::AccelerationStructure(_displayCore.device(), tlasCreate);

    auto scratch = getScratch(_displayCore.physicalDevice(), _displayCore.device(), tlas);
    auto scratchAddress = vku::getBufferAddress(_displayCore.device(), scratch.buffer);

    auto commandBuffers = vku::CommandBuffers::beginSingle(_displayCore.device(), _renderCore.commandPool());
    auto cmd = commandBuffers[0];

    auto instancesSize = sizeof(VkAccelerationStructureInstanceKHR) * instances.size();
    auto instancesBuffer = getRTBuffer(_displayCore.physicalDevice(), _displayCore.device(), instancesSize);
    auto instancesAddress = vku::getBufferAddress(_displayCore.device(), instancesBuffer.buffer);

    auto staging = vku::stagingBuffer(
        _displayCore.physicalDevice(),
        _displayCore.device(),
        instancesSize,
        instances.data());
    vku::deviceDeviceCopy(cmd, staging.buffer, instancesBuffer.buffer, instancesSize);

    VkMemoryBarrier barrier = {};
    barrier.sType = VK_STRUCTURE_TYPE_MEMORY_BARRIER;
    barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
    barrier.dstAccessMask = VK_ACCESS_ACCELERATION_STRUCTURE_WRITE_BIT_KHR;
    vkCmdPipelineBarrier(
        cmd,
        VK_PIPELINE_STAGE_TRANSFER_BIT, // srcStage
        VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR,
        0,
        1,
        &barrier,
        0,
        nullptr,
        0,
        nullptr);

    VkAccelerationStructureGeometryDataKHR geometryData = {};
    geometryData.instances.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_INSTANCES_DATA_KHR;
    geometryData.instances.data.deviceAddress = instancesAddress;

    VkAccelerationStructureGeometryKHR geometry = {};
    geometry.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR;
    geometry.geometryType = VK_GEOMETRY_TYPE_INSTANCES_KHR;
    geometry.geometry = geometryData;

    const VkAccelerationStructureGeometryKHR *pGeometries = &geometry;
    VkAccelerationStructureBuildGeometryInfoKHR buildInfo = {};
    buildInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR;
    buildInfo.flags = tlasCreate.flags;
    buildInfo.update = VK_FALSE;
    buildInfo.srcAccelerationStructure = VK_NULL_HANDLE;
    buildInfo.dstAccelerationStructure = tlas;
    buildInfo.geometryArrayOfPointers = VK_FALSE;
    buildInfo.geometryCount = 1;
    buildInfo.ppGeometries = &pGeometries;
    buildInfo.scratchData.deviceAddress = scratchAddress;

    VkAccelerationStructureBuildOffsetInfoKHR offset = {};
    offset.primitiveCount = static_cast<uint32_t>(instances.size());
    const VkAccelerationStructureBuildOffsetInfoKHR *pOffset = &offset;
    vkCmdBuildAccelerationStructureKHR(cmd, 1, &buildInfo, &pOffset);
    vkEndCommandBuffer(cmd);
    vku::CommandBuffers::submitSingle(commandBuffers, _displayCore.queue());

    return tlas;
}
