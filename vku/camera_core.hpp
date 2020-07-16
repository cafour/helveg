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
    static const float piHalf;
    static const float maxSpeed;
    static const float acceleration;
    static const glm::vec3 worldUp;

    vku::DisplayCore &_displayCore;
    std::vector<vku::Buffer> _cameraBuffers;
    std::vector<vku::DeviceMemory> _cameraMemories;
    vku::CameraView _view = {};
    glm::vec3 _front = glm::vec3(0.0f);
    glm::vec3 _right = glm::vec3(0.0f);
    glm::vec3 _up = glm::vec3(0.0f);
    glm::vec3 _move = glm::vec3(0.0f);
    double _lastPress = 0.0f;
    double _lastUpdate = 0.0f;
    float _speed = 0.0f;
    float _yaw = 0.0f;
    float _pitch = 0.0f;
    float _lastX = 0.0f;
    float _lastY = 0.0f;
    float _sensitivity = 0.005f;
    bool _enabled = false;

    void onMouseMove(double x, double y);
    void onMouseButton(int button, int action, int mods);
    void onKey(int key, int scancode, int action, int mods);
    void onFocus(int focused);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    CameraCore(vku::DisplayCore &displayCore, vku::RenderCore &renderCore);

    std::vector<vku::Buffer> &cameraBuffers() { return _cameraBuffers; }
    std::vector<vku::DeviceMemory> &cameraMemories() { return _cameraMemories; }
    vku::CameraView &view() { return _view; }
};
}
