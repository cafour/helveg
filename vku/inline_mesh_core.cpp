#include "inline_mesh_core.hpp"

vku::InlineMeshCore::InlineMeshCore(
    vku::TransferCore &transferCore,
    const void *vertices,
    size_t verticesSize,
    const uint32_t *indices,
    size_t indexCount)
    : _indexCount(indexCount)
{
    _vertexBuffer = vku::Buffer::exclusive(
        transferCore.device(),
        verticesSize,
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
    _vertexMemory = vku::DeviceMemory::deviceLocalData(
        transferCore.physicalDevice(),
        transferCore.device(),
        transferCore.transferPool(),
        transferCore.transferQueue(),
        _vertexBuffer,
        vertices,
        verticesSize);

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
