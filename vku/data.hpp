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

    struct Emitter
    {
        glm::vec3 position;
        float radius;
    };

    struct Label
    {
        char *text;
        glm::vec3 position;
        glm::vec2 size;
    };

    struct World
    {
        Chunk *chunks;
        glm::vec3 *chunkOffsets;
        uint32_t chunkCount;
        Emitter *fires;
        uint32_t fireCount;
        glm::vec3 skyColour;
        glm::vec3 initialCameraPosition;
        Label *labels;
        uint32_t labelCount;
    };

    struct Light
    {
        glm::vec4 position;
        glm::vec4 ambientColor;
        glm::vec4 diffuseColor;
        glm::vec4 specularColor;
    };

    struct Particle
    {
        glm::vec3 position;
        float pad;
        glm::vec4 color;
    };

    struct Time
    {
        float sec;
        float secDelta;
    };

    struct SimpleView {
        alignas(16) glm::mat4 model;
        alignas(16) glm::mat4 view;
        alignas(16) glm::mat4 projection;
    };

    struct CameraView {
        alignas(16) glm::mat4 view;
        alignas(16) glm::mat4 projection;
        alignas(16) glm::mat4 viewInverse;
        alignas(16) glm::mat4 projectionInverse;
        glm::vec3 position;
    };

    struct Model {
        alignas(16) glm::mat4 view;
    };
}
