#include "interop.hpp"

#include <cstdlib>
#include <iostream>
#include <stdexcept>

int helloTriangle()
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        vku::TriangleRender app(1280, 720);
        app.renderCore().run();
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int helloMesh(vku::MeshRender::Mesh mesh)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        vku::MeshRender app(1280, 720, mesh);
        app.renderCore().run();
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int helloGraph(vku::GraphRender::Graph graph)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        vku::GraphRender app(1280, 720, graph);
        app.renderCore().run();
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int createGraphRender(vku::GraphRender::Graph graph, void **ptr)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        auto graphRender = new vku::GraphRender(1024, 1024, graph);
        graphRender->renderCore().resize();
        *ptr = graphRender;
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int stepGraphRender(void *ptr)
{
    vku::GraphRender *graphRender = static_cast<vku::GraphRender *>(ptr);
    graphRender->flushPositions();
    glfwPollEvents();
    graphRender->renderCore().step();
    return glfwWindowShouldClose(graphRender->displayCore().window());
}

int destroyGraphRender(void *ptr)
{
    vku::GraphRender *graphRender = static_cast<vku::GraphRender *>(ptr);
    ENSURE(vkDeviceWaitIdle(graphRender->displayCore().device()));
    delete graphRender;
    return EXIT_SUCCESS;
}

int helloChunk(vku::ChunkRender::Chunk chunk)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        vku::ChunkRender app(1280, 720, chunk);
        app.renderCore().run();
    } catch (const std::exception &e) {
        std::cerr << std::string(e.what()) << std::endl;
        std::cout.flush();
        std::cerr.flush();
        return EXIT_FAILURE;
    }

    std::cout.flush();
    std::cerr.flush();
    return EXIT_SUCCESS;
}
