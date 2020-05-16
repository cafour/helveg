#include "vki.hpp"

#include <cstdlib>

int helloTriangle()
{
    if (volkInitialize() != VK_SUCCESS) {
        return EXIT_FAILURE;
    }
    try {
        Triangle app(1280, 720);
        app.run();
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int helloMesh(MeshRender::Mesh mesh)
{
    if (volkInitialize() != VK_SUCCESS) {
        return EXIT_FAILURE;
    }
    try {
        MeshRender app(1280, 720, mesh);
        app.run();
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int helloGraph(GraphRender::Graph graph)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        GraphRender app(1280, 720, graph);
        app.renderCore().run();
    } catch (const std::exception &e) {
        std::cerr << e.what() << std::endl;
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int createGraphRender(GraphRender::Graph graph, void **ptr)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        auto graphRender = new GraphRender(1280, 720, graph);
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
    GraphRender *graphRender = static_cast<GraphRender *>(ptr);
    glfwPollEvents();
    graphRender->renderCore().step();
    return glfwWindowShouldClose(graphRender->displayCore().window());
}

int destroyGraphRender(void *ptr)
{
    GraphRender *graphRender = static_cast<GraphRender *>(ptr);
    delete graphRender;
    return EXIT_SUCCESS;
}
