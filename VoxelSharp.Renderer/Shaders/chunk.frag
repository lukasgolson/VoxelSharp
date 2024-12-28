#version 330 core

layout (location = 0) out vec4 fragColor;

in vec4 voxel_color;
in vec2 frag_uv;
in vec3 frag_normal;

// Optional texture sampler (if you have a texture atlas)
//uniform sampler2D atlas;

// Simple lighting uniforms
vec3 lightDirection = normalize(vec3(0.3, -1.0, 0.2));
uniform vec3 lightColour = vec3(1.0, 1.0, 1.0);
uniform float ambientFactor = 0.1;

// Optional gamma correction
const float gamma = 2.2;

void main()
{
    // If using a texture atlas, multiply the sampled colour by the voxel colour.
    // Otherwise, just use voxel_color.
    //vec4 texColour = texture(atlas, frag_uv) * voxel_color;
    vec4 texColour = voxel_color;

    // Simple Lambertian lighting
    vec3 N = normalize(frag_normal);
    vec3 L = normalize(-lightDirection); // negative if light is from "above"
    float diffuse = max(dot(N, L), 0.0);

    // Combine diffuse with an ambient term
    vec3 lighting = ambientFactor + (diffuse * lightColour);

    // Final colour (before gamma correction)
    vec4 finalColour = vec4(texColour.rgb * lighting, texColour.a);

    // (Optional) Apply gamma correction
    finalColour.rgb = pow(finalColour.rgb, vec3(1.0 / gamma));

    fragColor = finalColour;
}
