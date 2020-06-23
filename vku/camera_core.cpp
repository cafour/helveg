#include "camera_core.hpp"

#include "cores.hpp"

#include <glm/gtx/rotate_vector.hpp>

vku::CameraCore::CameraCore(vku::DisplayCore &displayCore, vku::RenderCore &renderCore)
    : _displayCore(displayCore)
{
    using namespace std::placeholders;
    renderCore.onUpdate(std::bind(&vku::CameraCore::onUpdate, this, _1));
    renderCore.onResize(std::bind(&vku::CameraCore::onResize, this, _1, _2));
    displayCore.window().onMouseMove(std::bind(&vku::CameraCore::onMouseMove, this, _1, _2));
    displayCore.window().onKeyPress(std::bind(&vku::CameraCore::onKeyPress, this, _1, _2, _3, _4));
}

void vku::CameraCore::onMouseMove(double x, double y)
{
    float dx = static_cast<float>(x) - _lastX;
    float dy = static_cast<float>(y) - _lastY;

    _eye = glm::rotateY(glm::rotateX(_eye, dx * _sensitivity), dy * _sensitivity);
}

void vku::CameraCore::onKeyPress(int key, int scancode, int action, int mods)
{
    (void)scancode;
    (void)action;
    (void)mods;
    switch (key) {
    case GLFW_KEY_UP:
    case GLFW_KEY_W:
        _view.position += _eye;
        break;
    case GLFW_KEY_DOWN:
    case GLFW_KEY_S:
        _view.position -= _eye;
        break;
    case GLFW_KEY_LEFT:
    case GLFW_KEY_A:
        // TODO
        break;
    case GLFW_KEY_RIGHT:
    case GLFW_KEY_D:
        // TODO
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
        0.1f,
        100.0f);
    _view.projection[1][1] *= -1;
}

void vku::CameraCore::onUpdate(vku::SwapchainFrame &frame)
{
    _view.view = glm::lookAt(_view.position, _view.position + _eye, glm::vec3(0.0f, 1.0f, 0.0f));
    vku::hostDeviceCopy(_displayCore.device(), &_view, _cameraMemories[frame.index], sizeof(vku::CameraView));
}
