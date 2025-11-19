#version 330 core

// 1. Update Input Layout
// loc 0 is now a single integer containing x,y,z,face
layout (location = 0) in uint in_data;
layout (location = 1) in vec4 in_color;

uniform mat4 m_model;
uniform mat4 m_view;
uniform mat4 m_projection;

out vec4 voxel_color;
out vec2 frag_uv;
flat out vec3 frag_normal;

const vec2 uv_coords[4] = vec2[4](
vec2(0.0, 0.0),
vec2(0.0, 1.0),
vec2(1.0, 0.0),
vec2(1.0, 1.0)
);

const int uv_indices[12] = int[12](
1, 0, 2, 1, 2, 3,
3, 0, 2, 3, 1, 0
);

void main()
{
    // 2. Decode Data
    // Bitmasks: 0x1F = 31 (5 bits), 0x7 = 7 (3 bits)
    float x = float(in_data & 0x1Fu);
    float y = float((in_data >> 5) & 0x1Fu);
    float z = float((in_data >> 10) & 0x1Fu);
    int face_id = int((in_data >> 15) & 0x7u);

    voxel_color = in_color;

    int index = gl_VertexID % 6;
    frag_uv = uv_coords[uv_indices[index]];

    if (face_id == 0)      frag_normal = vec3(0.0, 1.0, 0.0); // Top
    else if (face_id == 1) frag_normal = vec3(0.0, -1.0, 0.0); // Bottom
    else if (face_id == 2) frag_normal = vec3(1.0, 0.0, 0.0); // Right
    else if (face_id == 3) frag_normal = vec3(-1.0, 0.0, 0.0); // Left
    else if (face_id == 4) frag_normal = vec3(0.0, 0.0, -1.0); // Back
    else if (face_id == 5) frag_normal = vec3(0.0, 0.0, 1.0);  // Front

    gl_Position = m_projection * m_view * m_model * vec4(x, y, z, 1.0);
}