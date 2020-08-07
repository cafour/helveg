#pragma once

#include "wrapper.hpp"
#include "transfer_core.hpp"

#include <glm/glm.hpp>

#include <optional>

namespace vku {

struct Chunk;

class MeshCore {
private:
    vku::Buffer _vertexBuffer;
    vku::DeviceMemory _vertexMemory;
    size_t _vertexCount;
    vku::Buffer _indexBuffer;
    vku::DeviceMemory _indexMemory;
    size_t _indexCount;
    bool _isColored;

public:
    MeshCore(
        vku::TransferCore &transferCore,
        const glm::vec3 *vertices,
        size_t vertexCount,
        const uint32_t *indices,
        size_t indexCount,
        const glm::vec3 *colors = nullptr,
        bool addressable = false);

    static std::optional<MeshCore> fromChunk(
        vku::TransferCore& transferCore,
        vku::Chunk chunk,
        bool addresable = false);

    vku::Buffer &vertexBuffer() { return _vertexBuffer; }
    vku::DeviceMemory &vertexMemory() { return _vertexMemory; }
    size_t vertexCount() { return _vertexCount; }
    vku::Buffer &indexBuffer() { return _indexBuffer; }
    vku::DeviceMemory &indexMemory() { return _indexMemory; }
    size_t indexCount() { return _indexCount; }
    bool hasColors() { return _isColored; }
};
}
