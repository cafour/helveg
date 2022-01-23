#pragma once

#include "cores.hpp"

#include <vector>

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
 * A version of WorldRender that uses the VK_KHR_ray_tracing_pipeline extension.
 **/
class RTRender {
private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::TransferCore _transferCore;
    vku::CameraCore _cameraCore;

    std::vector<vku::MeshCore> _meshes;

    VkPhysicalDeviceRayTracingPipelinePropertiesKHR _rtProperties;
    std::vector<vku::BackedBuffer> _blasBuffers;
    std::vector<vku::AccelerationStructure> _blases;
    vku::BackedBuffer _tlasBuffer;
    vku::AccelerationStructure _tlas;
    vku::DescriptorSetLayout _rayTraceDsl;
    vku::DescriptorPool _rayTraceDP;
    VkDescriptorSet _rayTraceDS;
    vku::PipelineLayout _rayTracePL;
    vku::RayTracingPipeline _rayTracePipeline;
    vku::BackedBuffer _sbt;
    vku::BackedImage _offscreen;
    uint32_t _shaderGroupCount = 0;

    vku::World _world;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);

    vku::GeometryInfo createGeometry(vku::MeshCore &mesh);
    void createBlas(vku::GeometriesInfo &geometries);
    VkAccelerationStructureInstanceKHR createASInstance(
        VkAccelerationStructureKHR as,
        glm::mat4 transform,
        uint32_t instanceId);
    void createTlas(std::vector<VkAccelerationStructureInstanceKHR> &instances);
    void createRTDescriptorSet();
    void createRTPipeline();
    void createSbt(uint32_t groupCount);

public:
    RTRender(int width, int height, World world, const std::string &title, bool debug = false);
    ~RTRender();

    vku::RenderCore &renderCore() { return _renderCore; }
    vku::DisplayCore &displayCore() { return _displayCore; }
};
}
