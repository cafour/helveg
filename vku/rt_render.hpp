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
    std::vector<vku::AccelerationStructure> _blases;
    vku::AccelerationStructure _tlas;
    vku::DescriptorSetLayout _rayTraceDsl;
    vku::DescriptorPool _rayTraceDP;
    VkDescriptorSet _rayTraceDS;
    vku::ImageView _offscreenColorIV;
    vku::PipelineLayout _rayTracePL;
    vku::RayTracingPipeline _rayTracePipeline;
    vku::BackedBuffer _sbt;

    vku::World _world;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);

    vku::GeometryInfo createGeometry(vku::MeshCore &mesh);
    vku::AccelerationStructure createBlas(vku::GeometriesInfo &geometries);
    VkAccelerationStructureInstanceKHR createASInstance(
        VkAccelerationStructureKHR as,
        glm::mat4 transform,
        uint32_t instanceId);
    vku::AccelerationStructure createTlas(std::vector<VkAccelerationStructureInstanceKHR> &instances);
    void createRTDescriptorSet();
    void createRTPipeline();
    void createSbt(uint32_t groupCount);

public:
    RTRender(int width, int height, World world, const std::string &title, bool debug = false);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}
