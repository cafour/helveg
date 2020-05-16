#pragma once

#include "triangle.hpp"
#include "mesh_render.hpp"
#include "graph_render.hpp"

#if defined _WIN32
    #define EXPORT __declspec(dllexport)
#else
    #define EXPORT __attribute__ ((visibility ("default")))
#endif

extern "C" {
    EXPORT int helloTriangle();

    EXPORT int helloMesh(MeshRender::Mesh mesh);

    EXPORT int helloGraph(GraphRender::Graph *graph);
}
