#include "mesh_core.hpp"
#include "data.hpp"

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

static void pushCube(
    std::vector<glm::vec3> &vertices,
    std::vector<glm::vec3> &colors,
    std::vector<uint32_t> &indices,
    glm::vec3 position,
    glm::vec3 color)
{
    static const glm::vec3 cubeVertices[] = {
        glm::vec3(0.0f, 0.0f, 0.0f),
        glm::vec3(1.0f, 0.0f, 0.0f),
        glm::vec3(1.0f, 0.0f, 1.0f),
        glm::vec3(0.0f, 0.0f, 1.0f),
        glm::vec3(0.0f, 1.0f, 0.0f),
        glm::vec3(1.0f, 1.0f, 0.0f),
        glm::vec3(1.0f, 1.0f, 1.0f),
        glm::vec3(0.0f, 1.0f, 1.0f)
    };

    // clang-format off
    static const uint32_t cubeIndices[] = { //
        0, 1, 3, 1, 2, 3,
        7, 5, 4, 7, 6, 5,
        4, 1, 0, 4, 5, 1,
        3, 2, 7, 2, 6, 7,
        5, 2, 1, 5, 6, 2,
        0, 3, 4, 3, 7, 4,
    };
    // clang-format on

    uint32_t last = static_cast<uint32_t>(vertices.size());
    for (size_t i = 0; i < sizeof(cubeVertices) / sizeof(glm::vec3); ++i) {
        vertices.push_back(cubeVertices[i] + position);
        colors.push_back(color);
    }

    for (size_t i = 0; i < sizeof(cubeIndices) / sizeof(uint32_t); ++i) {
        indices.push_back(cubeIndices[i] + last);
    }
}

vku::MeshCore vku::MeshCore::fromChunk(vku::TransferCore &transferCore, vku::Chunk chunk)
{
    if (!chunk.palette || !chunk.voxels || !chunk.size) {
        throw std::invalid_argument("the chunk is not valid");
    }

    std::vector<glm::vec3> vertices;
    std::vector<glm::vec3> colors;
    std::vector<uint32_t> indices;

    size_t plane = chunk.size * chunk.size;
    for (size_t x = 0; x < chunk.size; ++x) {
        for (size_t y = 0; y < chunk.size; ++y) {
            for (size_t z = 0; z < chunk.size; ++z) {
                size_t i = z + y * chunk.size + x * plane;
                auto block = chunk.voxels[i];
                if (block.flags & vku::BlockFlags::IS_AIR) {
                    continue;
                }
                pushCube(vertices, colors, indices, glm::vec3(x, y, z), chunk.palette[block.paletteIndex]);
            }
        }
    }

    return vku::MeshCore(transferCore, vertices.data(), vertices.size(), indices.data(), indices.size(), colors.data());
}
