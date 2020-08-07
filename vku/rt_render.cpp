#include "rt_render.hpp"

#include <vector>

/*
 * Based on the VK_KHR_ray_tracing tutorial by NVIDIA:
 * https://nvpro-samples.github.io/vk_raytracing_tutorial_KHR/
 */

const std::vector<const char*> requiredDevicesExtensions = std::vector<const char*>{
    VK_KHR_SWAPCHAIN_EXTENSION_NAME,
    VK_KHR_RAY_TRACING_EXTENSION_NAME,
    VK_KHR_MAINTENANCE3_EXTENSION_NAME,
    VK_KHR_PIPELINE_LIBRARY_EXTENSION_NAME,
    VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME,
    VK_KHR_BUFFER_DEVICE_ADDRESS_EXTENSION_NAME
};

const VkPhysicalDeviceRayTracingFeaturesKHR rtFeatures = {
    VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_TRACING_FEATURES_KHR
};

vku::RTRender::RTRender(int width, int height, World world, const std::string &title, bool debug)
    : _instanceCore("RTRender", true, true)
    , _displayCore(_instanceCore.instance(), width, height, "RTRender", requiredDevicesExtensions, nullptr, &rtFeatures)
    , _swapchainCore(_displayCore)
    , _renderCore(
        _displayCore,
        _swapchainCore,
        [this](auto &f) { return createFramebuffer(f); },
        [this](auto cb, auto &f) { recordCommandBuffer(cb, f); })
    , _rtProperties(vku::findProperties<VkPhysicalDeviceRayTracingPropertiesKHR>(
        _displayCore.physicalDevice(),
        VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_TRACING_PROPERTIES_KHR))
{
}

void vku::RTRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
}

vku::Framebuffer vku::RTRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    return {};
}

void vku::RTRender::createBlas(vku::MeshCore &mesh)
{
    // prepare a single geometry object

    VkAccelerationStructureCreateGeometryTypeInfoKHR geometryCreate = {};
    geometryCreate.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_GEOMETRY_TYPE_INFO_KHR;
    geometryCreate.geometryType = VK_GEOMETRY_TYPE_TRIANGLES_KHR;
    geometryCreate.indexType = VK_INDEX_TYPE_UINT32;
    geometryCreate.vertexFormat = VK_FORMAT_R32G32B32_SFLOAT;
    geometryCreate.maxPrimitiveCount = mesh.indexCount() / 3;
    geometryCreate.maxVertexCount = mesh.vertexCount();
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

    // create the blas

    VkAccelerationStructureCreateInfoKHR blasCreate = {};
    blasCreate.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR;
    blasCreate.flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
    blasCreate.maxGeometryCount = 1; // TODO: Change once you actually create more than one geometry.
    blasCreate.pGeometryInfos = &geometryCreate;

    _blas = vku::AccelerationStructure(_displayCore.device(), blasCreate);
}