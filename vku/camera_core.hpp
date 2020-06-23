#pragma once

#include "data.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

namespace vku {

class RenderCore;
class DisplayCore;
struct SwapchainFrame;

class CameraCore {
private:
    vku::DisplayCore &_displayCore;
    std::vector<vku::Buffer> _cameraBuffers;
    std::vector<vku::DeviceMemory> _cameraMemories;
    vku::CameraView _view = {};
    glm::vec3 _eye = glm::vec3(0.0f, 0.0f, 1.0f);
    float _lastX = -1.0;
    float _lastY = -1.0;
    float _sensitivity = 0.005;

    void onMouseMove(double x, double y);
    void onKeyPress(int key, int scancode, int action, int mods);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    CameraCore(vku::DisplayCore &displayCore, vku::RenderCore &renderCore);

    std::vector<vku::Buffer> &cameraBuffers() { return _cameraBuffers; }
    std::vector<vku::DeviceMemory> &cameraMemories() { return _cameraMemories; }
    vku::CameraView &view() { return _view; }
};
}
