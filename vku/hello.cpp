#include "vki.hpp"

int main()
{
    glm::vec2 positions[] = {
        glm::vec2(0.0f, 0.0f)
    };

    GraphRender::Graph graph { positions, nullptr, 1 };

    return helloGraph(graph);
}
