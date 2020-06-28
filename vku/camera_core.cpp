#include "camera_core.hpp"

#include "cores.hpp"

#include <glm/gtc/matrix_transform.hpp>

#include <iostream>

const float vku::CameraCore::piHalf = static_cast<float>(M_PI_2);
const float vku::CameraCore::maxSpeed = 0.125f;
const float vku::CameraCore::acceleration = 0.0625f;
const glm::vec3 vku::CameraCore::worldUp = glm::vec3(0.0f, 1.0f, 0.0f);

vku::CameraCore::CameraCore(vku::DisplayCore &displayCore, vku::RenderCore &renderCore)
    : _displayCore(displayCore)
{
    using namespace std::placeholders;
    renderCore.onUpdate(std::bind(&vku::CameraCore::onUpdate, this, _1));
    renderCore.onResize(std::bind(&vku::CameraCore::onResize, this, _1, _2));
    displayCore.window().onMouseMove(std::bind(&vku::CameraCore::onMouseMove, this, _1, _2));
    displayCore.window().onKeyPress(std::bind(&vku::CameraCore::onKeyPress, this, _1, _2, _3, _4));
    glm::dvec2 mousePosition = displayCore.window().mousePosition();
    _lastX = static_cast<float>(mousePosition.x);
    _lastY = static_cast<float>(mousePosition.y);
    _lastPress = glfwGetTime();
    _lastUpdate = _lastPress;
    displayCore.window().disableCursor();
}

void vku::CameraCore::onMouseMove(double x, double y)
{
    float dx = static_cast<float>(x) - _lastX;
    float dy = _lastY - static_cast<float>(y);
    _lastX = static_cast<float>(x);
    _lastY = static_cast<float>(y);

    _yaw += _sensitivity * dx;
    _pitch += _sensitivity * dy;
    _pitch = std::clamp(_pitch, -piHalf + 0.05f, piHalf - 0.05f);
}

void vku::CameraCore::onKeyPress(int key, int scancode, int action, int mods)
{
    (void)scancode;
    (void)mods;
    if (action == GLFW_PRESS && _move == glm::ivec2(0)) {
        _lastPress = glfwGetTime();
    }

    int value = action == GLFW_PRESS || action == GLFW_REPEAT ? 1 : 0;
    switch (key) {
    case GLFW_KEY_W:
    case GLFW_KEY_UP:
        _move.x = value;
        break;
    case GLFW_KEY_S:
    case GLFW_KEY_DOWN:
        _move.x = -value;
        break;
    case GLFW_KEY_A:
    case GLFW_KEY_LEFT:
        _move.y = -value;
        break;
    case GLFW_KEY_D:
    case GLFW_KEY_RIGHT:
        _move.y = value;
        break;
    }
}

void vku::CameraCore::onResize(size_t imageCount, VkExtent2D extent)
{
    for (size_t i = _cameraBuffers.size(); i < imageCount; ++i) {
        auto buffer = vku::Buffer::exclusive(
            _displayCore.device(),
            sizeof(vku::SimpleView),
            VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
        auto memory = vku::DeviceMemory::hostCoherentBuffer(
            _displayCore.physicalDevice(),
            _displayCore.device(),
            buffer);
        _cameraBuffers.emplace_back(std::move(buffer));
        _cameraMemories.emplace_back(std::move(memory));
    }
    _cameraBuffers.resize(imageCount);
    _cameraMemories.resize(imageCount);

    _view.projection = glm::perspective(
        glm::radians(60.0f),
        static_cast<float>(extent.width) / static_cast<float>(extent.height),
        0.01f,
        1000.0f);
    _view.projection[1][1] *= -1;
}

void vku::CameraCore::onUpdate(vku::SwapchainFrame &frame)
{
    _front.x = std::cos(_yaw) * std::cos(_pitch);
    _front.y = std::sin(_pitch);
    _front.z = std::sin(_yaw) * std::cos(_pitch);
    _front = glm::normalize(_front);
    _right = glm::normalize(glm::cross(_front, worldUp));
    _up = glm::normalize(glm::cross(_right, _front));

    float duration = static_cast<float>(glfwGetTime() - _lastPress);
    float speed = std::clamp(duration * acceleration, 0.0f, maxSpeed);
    double time = glfwGetTime();
    float diff = static_cast<float>(time - _lastUpdate);
    _lastUpdate = time;
    _view.position += (_front * static_cast<float>(_move.x) + _right * static_cast<float>(_move.y)) * speed * diff;

    _view.view = glm::lookAt(_view.position, _view.position + _front, _up);
    vku::hostDeviceCopy(_displayCore.device(), &_view, _cameraMemories[frame.index], sizeof(vku::CameraView));
}
