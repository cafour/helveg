#pragma once

#include "transfer_core.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

namespace vku {
class InlineMeshCore {
private:
    vku::Buffer _vertexBuffer;
    vku::DeviceMemory _vertexMemory;
    vku::Buffer _indexBuffer;
    vku::DeviceMemory _indexMemory;
    size_t _indexCount;

public:
    InlineMeshCore(
        vku::TransferCore &transferCore,
        const void *vertices,
        size_t verticesSize,
        const uint32_t *indices,
        size_t indexCount);

    vku::Buffer &vertexBuffer() { return _vertexBuffer; }
    vku::DeviceMemory &vertexMemory() { return _vertexMemory; }
    vku::Buffer &indexBuffer() { return _indexBuffer; }
    vku::DeviceMemory &indexMemory() { return _indexMemory; }
    size_t indexCount() { return _indexCount; }

    static InlineMeshCore cube(vku::TransferCore &transferCore);
};
}
