#include "interop.hpp"
#include "log.hpp"
#include "rt_render.hpp"

#include <cstdlib>
#include <functional>
#include <iostream>
#include <stdexcept>

bool isDebug = false;
bool isRayTraced = false;

void setDebug(bool debug)
{
    isDebug = debug;
}

void setRayTracing(bool rayTracing)
{
    isRayTraced = rayTracing;
}

void setLogCallback(void (*callback)(int level, const char *message))
{
    vku::onLog([callback](vku::LogLevel l, const std::string &m) {
        callback(l, m.c_str());
    });
}

static int hello(std::function<void()> run)
{
    if (volkInitialize() != VK_SUCCESS || glfwInit() == GLFW_FALSE) {
        return EXIT_FAILURE;
    }
    try {
        run();
    } catch (const std::exception &e) {
        vku::logCritical(e.what());
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}

int helloTriangle()
{
    return hello([]() {
        vku::TriangleRender app(1280, 720, isDebug);
        app.renderCore().run();
    });
}

int helloMesh(vku::MeshRender::Mesh mesh)
{
    return hello([mesh]() {
        vku::MeshRender app(1280, 720, mesh, isDebug);
        app.renderCore().run();
    });
}

int helloGraph(vku::GraphRender::Graph graph)
{
    return hello([graph]() {
        vku::GraphRender app(1280, 720, graph, isDebug);
        app.renderCore().run();
    });
}

int createGraphRender(vku::GraphRender::Graph graph, void **ptr)
{
    return hello([graph, ptr]() {
        auto graphRender = new vku::GraphRender(1024, 1024, graph, isDebug);
        graphRender->renderCore().resize();
        *ptr = graphRender;
    });
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

int helloChunk(vku::Chunk chunk)
{
    return hello([chunk]() {
        vku::ChunkRender app(1280, 720, chunk, isDebug);
        app.renderCore().run();
    });
}

int helloWorld(vku::World world, const char *title)
{
    return hello([world, title]() {
        if (isRayTraced) {
            vku::RTRender app(1280, 720, world, title, isDebug);
            ENSURE(vkDeviceWaitIdle(app.displayCore().device()));
            // app.renderCore().run();
        } else {
            vku::WorldRender app(1280, 720, world, title, isDebug);
            app.renderCore().run();
        }
    });
}
