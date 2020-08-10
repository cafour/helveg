#version 460
#extension GL_EXT_ray_tracing : require
#extension GL_EXT_nonuniform_qualifier : enable
#extension GL_EXT_scalar_block_layout : enable

hitAttributeEXT vec2 attribs;

// clang-format off
struct hitPayload {
    vec3 hitValue;
};
layout(location = 0) rayPayloadInEXT hitPayload prd;
layout(location = 1) rayPayloadEXT bool isShadowed;

layout(binding = 0, set = 0) uniform accelerationStructureEXT topLevelAS;
layout(binding = 3, set = 0, scalar) buffer Vertices { vec3 v[]; } vertices[];
layout(binding = 4, set = 0) buffer Indices { uint i[]; } indices[];
layout(binding = 5, set = 0, scalar) buffer Colors { vec3 c[]; } colors[];

// clang-format on

const vec4 clearColor = vec4(0.533f, 0.808f, 0.925f, 1.0f);
const vec3 lightPosition = vec3(10.f, 15.f, 8.f);
const float lightIntensity = 100.0f;
const int lightType = 0;

void main()
{
    // Object of this instance
    uint objId = gl_InstanceID;

    // Indices of the triangle
    ivec3 ind = ivec3(indices[nonuniformEXT(objId)].i[3 * gl_PrimitiveID + 0], //
        indices[nonuniformEXT(objId)].i[3 * gl_PrimitiveID + 1], //
        indices[nonuniformEXT(objId)].i[3 * gl_PrimitiveID + 2]); //
    // Vertex of the triangle
    vec3 v0 = vertices[nonuniformEXT(objId)].v[ind.x];
    vec3 v1 = vertices[nonuniformEXT(objId)].v[ind.y];
    vec3 v2 = vertices[nonuniformEXT(objId)].v[ind.z];

    const vec3 barycentrics = vec3(1.0 - attribs.x - attribs.y, attribs.x, attribs.y);

    // Computing the normal at hit position
    vec3 normal = normalize(cross(v1 - v2, v0 - v2));

    // Computing the coordinates of the hit position
    vec3 worldPos = v0 * barycentrics.x + v1 * barycentrics.y + v2 * barycentrics.z;

    // Vector toward the light
    vec3 L;
    float lightIntensity = lightIntensity;
    float lightDistance = 100000.0;
    // Point light
    if (lightType == 0) {
        vec3 lDir = lightPosition - worldPos;
        lightDistance = length(lDir);
        lightIntensity = lightIntensity / (lightDistance * lightDistance);
        L = normalize(lDir);
    } else // Directional light
    {
        L = normalize(lightPosition - vec3(0));
    }

    vec3 c = colors[nonuniformEXT(objId)].c[ind.x];
    // Diffuse
    float NdotL = max(dot(normal, L), 0.0f);
    vec3 diffuse = NdotL * c;

    vec3 specular = vec3(0);
    float attenuation = 1;

    // Tracing shadow ray only if the light is visible from the surface
    if (dot(normal, L) > 0) {
        float tMin = 0.001;
        float tMax = lightDistance;
        vec3 origin = gl_WorldRayOriginEXT + gl_WorldRayDirectionEXT * gl_HitTEXT;
        vec3 rayDir = L;
        uint flags = gl_RayFlagsTerminateOnFirstHitEXT | gl_RayFlagsOpaqueEXT
            | gl_RayFlagsSkipClosestHitShaderEXT;
        isShadowed = true;
        traceRayEXT(topLevelAS, // acceleration structure
            flags, // rayFlags
            0xFF, // cullMask
            0, // sbtRecordOffset
            0, // sbtRecordStride
            1, // missIndex
            origin, // ray origin
            tMin, // ray min range
            rayDir, // ray direction
            tMax, // ray max range
            1 // payload (location = 1)
        );

        if (isShadowed) {
            attenuation = 0.3;
        } else {
            // Specular
            vec3 H = normalize(L + gl_WorldRayDirectionEXT);
            float NdotH = max(dot(normal, H), 0.0f);
            specular = pow(NdotH, 10) * c;
        }
    }

    prd.hitValue = vec3(lightIntensity * attenuation * (diffuse + specular));
}
