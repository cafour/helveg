#pragma once

#include "data.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

#include <cmath>

/**
 * Simple camera with yaw and pitch.
 * Based on https://learnopengl.com/Getting-started/Camera.
 **/

namespace vku {

class RenderCore;
class DisplayCore;
struct SwapchainFrame;

class CameraCore {
private:
    const float piHalf = static_cast<float>(M_PI_2);
    vku::DisplayCore &_displayCore;
    std::vector<vku::Buffer> _cameraBuffers;
    std::vector<vku::DeviceMemory> _cameraMemories;
    vku::CameraView _view = {};
    glm::vec3 _front = {};
    glm::vec3 _right = {};
    glm::vec3 _up = {};
    const glm::vec3 _worldUp = glm::vec3(0.0f, 1.0f, 0.0f);
    float _yaw = piHalf;
    float _pitch = 0.0f;
    float _speed = 0.005f;
    float _lastX = 0.0f;
    float _lastY = 0.0f;
    float _sensitivity = 0.005f;

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
