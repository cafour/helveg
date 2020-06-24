#include "camera_core.hpp"

#include "cores.hpp"

#include <glm/gtc/matrix_transform.hpp>

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
    (void)action;
    (void)mods;
    switch (key) {
    case GLFW_KEY_UP:
    case GLFW_KEY_W:
        _view.position += _speed * _front;
        break;
    case GLFW_KEY_DOWN:
    case GLFW_KEY_S:
        _view.position -= _speed * _front;
        break;
    case GLFW_KEY_LEFT:
    case GLFW_KEY_A:
        _view.position -= _speed * _right;
        break;
    case GLFW_KEY_RIGHT:
    case GLFW_KEY_D:
        _view.position += _speed * _right;
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
        glm::radians(45.0f),
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
    _right = glm::normalize(glm::cross(_front, _worldUp));
    _up = glm::normalize(glm::cross(_right, _front));
    _view.view = glm::lookAt(_view.position, _view.position + _front, _up);
    vku::hostDeviceCopy(_displayCore.device(), &_view, _cameraMemories[frame.index], sizeof(vku::CameraView));
}
