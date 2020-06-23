#pragma once

#include <glm/glm.hpp>

/**
 * Structs transferred across interop with C++ or GLSL.
 **/

namespace vku {
    enum BlockFlags
    {
        IS_AIR = 1 << 0
    };

    struct Block
    {
        uint8_t paletteIndex;
        uint8_t flags;
    };

    struct Chunk
    {
        Block *voxels;
        glm::vec3 *palette;
        uint32_t size;
    };

    struct World
    {
        Chunk *chunks;
        glm::vec3 *positions;
        uint32_t count;
    };

    struct Light
    {
        glm::vec4 position;
        glm::vec4 color;
    };

    struct SimpleView {
        alignas(16) glm::mat4 model;
        alignas(16) glm::mat4 view;
        alignas(16) glm::mat4 projection;
    };

    struct CameraView {
        alignas(16) glm::mat4 view;
        alignas(16) glm::mat4 projection;
        glm::vec3 position;
    };

    struct Model {
        alignas(16) glm::mat4 view;
    };
}
