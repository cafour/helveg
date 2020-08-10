#version 460
#extension GL_EXT_ray_tracing : require

struct hitPayload {
    vec3 hitValue;
};
layout(location = 0) rayPayloadInEXT hitPayload prd;
// TODO: make clearColor a push constant
const vec4 clearColor = vec4(0.533f, 0.808f, 0.925f, 1.0f);

void main()
{
  prd.hitValue = clearColor.xyz * 0.8;
}
