#pragma once

#include "triangle_render.hpp"
#include "mesh_render.hpp"
#include "graph_render.hpp"
#include "chunk_render.hpp"

#if defined _WIN32
    #define EXPORT __declspec(dllexport)
#else
    #define EXPORT __attribute__ ((visibility ("default")))
#endif

extern "C" {
    EXPORT int helloTriangle();

    EXPORT int helloMesh(vku::MeshRender::Mesh mesh);

    EXPORT int helloGraph(vku::GraphRender::Graph graph);

    EXPORT int createGraphRender(vku::GraphRender::Graph graph, void **graphRender);

    EXPORT int stepGraphRender(void *ptr);

    EXPORT int destroyGraphRender(void *ptr);

    EXPORT int helloChunk(vku::ChunkRender::Chunk chunk);
}