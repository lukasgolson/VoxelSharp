#version 330 core

layout (location = 0) in vec3 in_position;
layout (location = 1) in vec4 in_color;
layout (location = 2) in int face_id;

uniform mat4 m_model;
uniform mat4 m_view;
uniform mat4 m_projection;

out vec4 voxel_color;
out vec3 frag_position; // Pass position to fragment shader

void main() {
    voxel_color = in_color;
    frag_position = vec3(m_model * vec4(in_position, 1.0)); // Transform position to world space
    gl_Position = m_projection * m_view * vec4(frag_position, 1.0);
}
