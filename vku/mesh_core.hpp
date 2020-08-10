#pragma once

#include "wrapper.hpp"
#include "transfer_core.hpp"

#include <glm/glm.hpp>

#include <optional>

namespace vku {

struct Chunk;

class MeshCore {
private:
    vku::BackedBuffer _vertices;
    size_t _vertexCount;
    vku::BackedBuffer _indices;
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
        bool isAddressable = false,
        bool isStorage = false);

    static std::optional<MeshCore> fromChunk(
        vku::TransferCore& transferCore,
        vku::Chunk chunk,
        bool isAddressable = false,
        bool isStorage = false);

    vku::Buffer &vertexBuffer() { return _vertices.buffer; }
    vku::DeviceMemory &vertexMemory() { return _vertices.memory; }
    size_t vertexCount() { return _vertexCount; }
    vku::Buffer &indexBuffer() { return _indices.buffer; }
    vku::DeviceMemory &indexMemory() { return _indices.memory; }
    size_t indexCount() { return _indexCount; }
    bool hasColors() { return _isColored; }
};
}
