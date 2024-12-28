#version 330 core

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec4 in_color;
layout (location = 2) in int face_id;

uniform mat4 m_model;
uniform mat4 m_view;
uniform mat4 m_projection;

out vec4 voxel_color;
out vec2 frag_uv;
out vec3 frag_normal;

// Predefined UV coordinates
const vec2 uv_coords[4] = vec2[4](
vec2(0.0, 0.0),
vec2(0.0, 1.0),
vec2(1.0, 0.0),
vec2(1.0, 1.0)
);

// Indices for even and odd faces.
// 6 vertices per face in a typical cube geometry (two triangles).
const int uv_indices[12] = int[12](
1, 0, 2, 1, 2, 3, // Tex coords indices for one orientation
3, 0, 2, 3, 1, 0    // Tex coords indices for the opposite orientation
);

void main()
{
    voxel_color = in_color;

    // Determine per-vertex UVs. gl_VertexID helps differentiate
    // among the 6 vertices in the two triangles of each face.
    // If you index vertices differently in your buffers, adjust accordingly.
    int index = gl_VertexID % 6;
    frag_uv = uv_coords[uv_indices[index]];

    // Assign normals based on face_id (0 to 5).
    // Adjust to match your mesh generation:
    if (face_id == 0)      frag_normal = vec3(0.0, 1.0, 0.0); // Top
    else if (face_id == 1) frag_normal = vec3(0.0, -1.0, 0.0); // Bottom
    else if (face_id == 2) frag_normal = vec3(1.0, 0.0, 0.0); // Right
    else if (face_id == 3) frag_normal = vec3(-1.0, 0.0, 0.0); // Left
    else if (face_id == 4) frag_normal = vec3(0.0, 0.0, -1.0); // Back
    else if (face_id == 5) frag_normal = vec3(0.0, 0.0, 1.0); // Front


    // Correct matrix multiplication order
    gl_Position = vec4(in_position, 1.0) * m_model * m_view * m_projection;

}
