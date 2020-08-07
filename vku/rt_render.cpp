#include "rt_render.hpp"

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
}

void vku::RTRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
}

vku::Framebuffer vku::RTRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    return {};
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
    blasCreate.maxGeometryCount = geometries.createInfos.size();
    blasCreate.pGeometryInfos = geometries.createInfos.data();
    auto blas = vku::AccelerationStructure(_displayCore.device(), blasCreate);

    VkAccelerationStructureMemoryRequirementsInfoKHR asReqs = {};
    asReqs.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_MEMORY_REQUIREMENTS_INFO_KHR;
    asReqs.type = VK_ACCELERATION_STRUCTURE_MEMORY_REQUIREMENTS_TYPE_BUILD_SCRATCH_KHR;
    asReqs.accelerationStructure = blas;
    asReqs.buildType = VK_ACCELERATION_STRUCTURE_BUILD_TYPE_DEVICE_KHR;

    VkMemoryRequirements2 memReqs = {};
    memReqs.sType = VK_STRUCTURE_TYPE_MEMORY_REQUIREMENTS_2;

    vkGetAccelerationStructureMemoryRequirementsKHR(_displayCore.device(), &asReqs, &memReqs);
    VkDeviceSize scratchSize = memReqs.memoryRequirements.size;

    asReqs.type = VK_ACCELERATION_STRUCTURE_MEMORY_REQUIREMENTS_TYPE_OBJECT_KHR;
    vkGetAccelerationStructureMemoryRequirementsKHR(_displayCore.device(), &asReqs, &memReqs);
    VkDeviceSize objectSize = memReqs.memoryRequirements.size;

    auto scratchBuffer = vku::Buffer::exclusive(
        _displayCore.device(),
        scratchSize,
        VK_BUFFER_USAGE_RAY_TRACING_BIT_KHR | VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT);
    auto scratchMemory = vku::DeviceMemory::deviceLocalBuffer(
        _displayCore.physicalDevice(),
        _displayCore.device(),
        scratchBuffer,
        VK_MEMORY_ALLOCATE_DEVICE_ADDRESS_BIT);
    VkDeviceAddress scratchAddress = vku::getBufferAddress(_displayCore.device(), scratchBuffer);

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

    std::vector<const VkAccelerationStructureBuildOffsetInfoKHR*> pOffsets(geometries.offsets.size());
    for(size_t i = 0; i < geometries.offsets.size(); ++i) {
        pOffsets[i] = &geometries.offsets[i];
    }
    vkCmdBuildAccelerationStructureKHR(cmd, 1, &buildInfo, pOffsets.data());
    vkEndCommandBuffer(cmd);
    vku::CommandBuffers::submitSingle(commandBuffers, _displayCore.queue());

    return blas;
}
