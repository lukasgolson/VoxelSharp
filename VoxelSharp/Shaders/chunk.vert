#version 330 core

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec4 in_color;
layout (location = 2) in int face_id;

uniform mat4 m_model;
uniform mat4 m_view;
uniform mat4 m_projection;

out vec4 voxel_color;


const vec2 uv_coords[4] = vec2[4](
vec2(0, 0), vec2(0, 1),
vec2(1, 0), vec2(1, 1)
);

const int uv_indices[12] = int[12](
1, 0, 2, 1, 2, 3, // tex coords indices for vertices of an even face
3, 0, 2, 3, 1, 0   // odd face
);


void main() {
    voxel_color = in_color;
    gl_Position = m_projection * m_view * m_model * vec4(in_position, 1.0);
}