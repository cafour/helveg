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
    vku::Chunk chunk,
    glm::ivec3 position,
    glm::vec3 color)
{
    static const std::array<glm::vec3, 8> cubeVertices = {
        glm::vec3(0.0f, 0.0f, 0.0f), // lower front left
        glm::vec3(1.0f, 0.0f, 0.0f), // lower front right
        glm::vec3(1.0f, 0.0f, 1.0f), // lower back right
        glm::vec3(0.0f, 0.0f, 1.0f), // lower back left
        glm::vec3(0.0f, 1.0f, 0.0f), // upper front left
        glm::vec3(1.0f, 1.0f, 0.0f), // upper front right
        glm::vec3(1.0f, 1.0f, 1.0f), // upper back right
        glm::vec3(0.0f, 1.0f, 1.0f) // upper back left
    };

    static const std::array<glm::ivec3, 6> neighbours = {
        glm::ivec3(0, -1, 0), // bottom
        glm::ivec3(0, 1, 0), // top
        glm::ivec3(0, 0, -1), // front
        glm::ivec3(0, 0, 1), // back
        glm::ivec3(1, 0, 0), // right
        glm::ivec3(-1, 0, 0), // left
    };

    static const std::array<std::array<uint32_t, 6>, 6> cubeIndices = {
        std::array<uint32_t, 6> { 0, 1, 3, 1, 2, 3 }, // bottom
        std::array<uint32_t, 6> { 7, 5, 4, 7, 6, 5 }, // top
        std::array<uint32_t, 6> { 4, 1, 0, 4, 5, 1 }, // front
        std::array<uint32_t, 6> { 3, 2, 7, 2, 6, 7 }, // back
        std::array<uint32_t, 6> { 5, 2, 1, 5, 6, 2 }, // right
        std::array<uint32_t, 6> { 0, 3, 4, 3, 7, 4 }, // left
    };

    std::array<uint32_t, 8> actualIndices = { ~0u, ~0u, ~0u, ~0u, ~0u, ~0u, ~0u, ~0u };

    for (size_t f = 0; f < neighbours.size(); ++f) {
        glm::ivec3 neighbour = position + neighbours[f];
        size_t neighbourVoxelIndex = neighbour.z + neighbour.y * chunk.size + neighbour.x * chunk.size * chunk.size;
        int chunkSize = static_cast<int>(chunk.size);
        if (neighbour.x >= 0 && neighbour.x < chunkSize
            && neighbour.y >= 0 && neighbour.y < chunkSize
            && neighbour.z >= 0 && neighbour.z < chunkSize
            && (chunk.voxels[neighbourVoxelIndex].flags & vku::BlockFlags::IS_AIR) == 0) {
            continue;
        }

        // push the appropriate face
        for (size_t i = 0; i < 6; ++i) {
            uint32_t localIndex = cubeIndices[f][i];
            if (actualIndices[localIndex] == ~0u)
            {
                vertices.push_back(cubeVertices[localIndex] + glm::vec3(position));
                colors.push_back(color);
                actualIndices[localIndex] = static_cast<uint32_t>(vertices.size() - 1);
            }

            indices.push_back(actualIndices[localIndex]);
        }
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
                pushCube(vertices, colors, indices, chunk, glm::ivec3(x, y, z), chunk.palette[block.paletteIndex]);
            }
        }
    }

    return vku::MeshCore(transferCore, vertices.data(), vertices.size(), indices.data(), indices.size(), colors.data());
}
