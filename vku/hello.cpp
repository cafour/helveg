#include "interop.hpp"

int main()
{
    glm::vec2 positions[] = {
        glm::vec2(0.0f, 0.0f)
    };

    vku::GraphRender::Graph graph { positions, nullptr, 1 };

    return helloGraph(graph);
}
