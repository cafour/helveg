#include "inline_mesh_core.hpp"

#include <glm/glm.hpp>

static const glm::vec3 vertices[] = {
    glm::vec3(1, 1, 1),
    glm::vec3(1, 1, -1),
    glm::vec3(1, -1, -1),
    glm::vec3(1, -1, 1),
    glm::vec3(-1, 1, 1),
    glm::vec3(-1, 1, -1),
    glm::vec3(-1, -1, -1),
    glm::vec3(-1, -1, 1)
};

static const uint32_t indices[] = {
    3, 2, 0, 0, 2, 1,
    2, 6, 1, 1, 6, 5,
    6, 7, 5, 5, 7, 4,
    7, 3, 4, 4, 3, 0,
    0, 1, 4, 4, 1, 5,
    2, 3, 6, 6, 3, 7
};

vku::InlineMeshCore vku::InlineMeshCore::cube(vku::TransferCore &transferCore)
{
    return vku::InlineMeshCore(transferCore, vertices, sizeof(vertices), indices, sizeof(indices) / sizeof(uint32_t));
}
