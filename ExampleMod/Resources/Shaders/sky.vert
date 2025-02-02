#version 330 core
layout (location = 0) in vec3 aPos; // Skybox cube vertices

out vec3 skyDirection; // Send direction to fragment shader

uniform mat4 view;       // View matrix (without translation)
uniform mat4 projection; // Projection matrix

void main()
{
    skyDirection = normalize(aPos); // Pass normalized position as direction
    gl_Position = vec4(aPos, 1.0) * view * projection;
    gl_Position = gl_Position.xyww;  // Forces depth to 1.0 (far plane)

}
