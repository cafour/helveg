#pragma once

#include "cores.hpp"

namespace vku {

struct GeometryInfo
{
    VkAccelerationStructureCreateGeometryTypeInfoKHR createInfo;
    VkAccelerationStructureGeometryKHR geometry;
    VkAccelerationStructureBuildOffsetInfoKHR offset;
};

struct GeometriesInfo
{
    std::vector<VkAccelerationStructureCreateGeometryTypeInfoKHR> createInfos;
    std::vector<VkAccelerationStructureGeometryKHR> geometries;
    std::vector<VkAccelerationStructureBuildOffsetInfoKHR> offsets;
};

/**
 * A version of WorldRender that uses the VK_KHR_ray_tracing extension.
 **/
class RTRender {
private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::TransferCore _transferCore;

    VkPhysicalDeviceRayTracingPropertiesKHR _rtProperties;
    vku::AccelerationStructure _blas;

    vku::World _world;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    vku::GeometryInfo createGeometry(vku::MeshCore &mesh);
    vku::AccelerationStructure createBlas(vku::GeometriesInfo &geometries);
    VkAccelerationStructureInstanceKHR createASInstance(
        VkAccelerationStructureKHR as,
        glm::mat4 transform,
        uint32_t instanceId);
    vku::AccelerationStructure createTlas(std::vector<VkAccelerationStructureInstanceKHR> &instances);

public:
    RTRender(int width, int height, World world, const std::string &title, bool debug = false);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}