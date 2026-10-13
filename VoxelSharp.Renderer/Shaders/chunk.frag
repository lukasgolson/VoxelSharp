#version 330 core

// Output color
out vec4 fragColor;

// Inputs from vertex shader
in vec4 voxel_color;
in vec2 frag_uv;
flat in vec3 frag_normal;

// Lighting uniforms
uniform vec3 lightDirection = vec3(0.5, 0.8, 0.3); // Direction of light
uniform vec3 lightColour = vec3(1.0, 1.0, 1.0); // Light color
uniform float ambientFactor = 0; // Ambient light factor
const float gamma = 2.2;

void main()
{
    // Simple Lambertian lighting
    vec3 N = normalize(frag_normal);
    vec3 L = normalize(-lightDirection); // Invert light direction for calculation
    float diffuse = max(dot(N, L), 0.0); // Calculate diffuse lighting

    // Darken faces based on the dot product
    float darkeningFactor = 0.5 + (0.5 * diffuse); // Adjust darkening factor
    vec3 lighting = (ambientFactor + (darkeningFactor * lightColour)); // Calculate final lighting

    vec4 finalColour;
    // Combine voxel color with lighting
    finalColour.rgb = voxel_color.rgb * lighting * voxel_color.a;
    finalColour.a = voxel_color.a;

    fragColor = finalColour;

}