#include "mesh_core.hpp"

vku::MeshCore::MeshCore(
    vku::TransferCore &transferCore,
    const glm::vec3 *vertices,
    size_t vertexCount,
    const uint32_t *indices,
    size_t indexCount,
    const glm::vec3 *colors)
    : _vertexCount(vertexCount)
    , _indexCount(indexCount)
{
    size_t vertexBufferSize = sizeof(glm::vec3) * vertexCount;
    size_t totalVertexBufferSize = vertexBufferSize;
    if (colors) {
        totalVertexBufferSize *= 2;
    }
    auto stagingBuffer = vku::Buffer::exclusive(
        transferCore.device(),
        totalVertexBufferSize,
        VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    auto stagingMemory = vku::DeviceMemory::hostCoherentBuffer(
        transferCore.physicalDevice(),
        transferCore.device(),
        stagingBuffer);
    vku::hostDeviceCopy(transferCore.device(), vertices, stagingMemory, vertexBufferSize);
    if (colors) {
        vku::hostDeviceCopy(transferCore.device(), colors, stagingMemory, vertexBufferSize, vertexBufferSize);
    }
    _vertexBuffer = vku::Buffer::exclusive(
        transferCore.device(),
        totalVertexBufferSize,
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
    _vertexMemory = vku::DeviceMemory::deviceLocalBuffer(
        transferCore.physicalDevice(),
        transferCore.device(),
        _vertexBuffer);
    vku::deviceDeviceCopy(
        transferCore.device(),
        transferCore.transferPool(),
        transferCore.transferQueue(),
        stagingBuffer,
        _vertexBuffer,
        totalVertexBufferSize);

    _indexBuffer = vku::Buffer::exclusive(
        transferCore.device(),
        indexCount * sizeof(uint32_t),
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT);
    _indexMemory = vku::DeviceMemory::deviceLocalData(
        transferCore.physicalDevice(),
        transferCore.device(),
        transferCore.transferPool(),
        transferCore.transferQueue(),
        _indexBuffer,
        indices,
        indexCount * sizeof(uint32_t));
}
