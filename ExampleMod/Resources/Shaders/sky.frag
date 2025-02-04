#version 330 core
in vec3 skyDirection; // Interpolated direction from vertex shader
out vec4 FragColor;

uniform float time; // 0.0 = midnight, 0.5 = noon, 1.0 = next midnight

// Gradient function to create smooth transitions
vec3 gradientSkyColor(float height, float time) {
    vec3 nightZenith = vec3(0.02, 0.02, 0.05); // Night sky (top)
    vec3 nightHorizon = vec3(0.1, 0.1, 0.15);  // Night horizon
    vec3 dayZenith = vec3(0.4, 0.7, 1.0);      // Bright blue (day top)
    vec3 dayHorizon = vec3(0.7, 0.85, 1.0);    // Light blue (day bottom)

    // Calculate rotation factor based on time (0 to 1)
    float rotation = time * 2.0 * 3.14159; // Full rotation over 1 day

    // Sine waves to create smooth transitions over time
    float zenithFactor = (sin(rotation + 0.0) + 1.0) * 0.5; // For zenith colors
    float horizonFactor = (sin(rotation + 3.14159) + 1.0) * 0.5; // For horizon colors

    // Interpolating colors based on height, creating a continuous gradient
    vec3 zenithColor = mix(nightZenith, dayZenith, zenithFactor);
    vec3 horizonColor = mix(nightHorizon, dayHorizon, horizonFactor);

    // Mix both colors for a smoother gradient
    return mix(horizonColor, zenithColor, smoothstep(0.0, 1.0, height));
}

// Random function for dithering
float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453);
}

// Main function
void main()
{
    // Normalize sky direction to get an up-down factor
    float heightFactor = normalize(skyDirection).y; // -1 (bottom) to +1 (top)
    heightFactor = (heightFactor + 1.0) * 0.5; // Convert range to [0,1]

    // Get the continuous sky color using the gradient function with time rotation
    vec3 skyColor = gradientSkyColor(heightFactor, time);

    // Subtle dithering
    float dither = (random(gl_FragCoord.xy * 0.1) - 0.5) * 0.01; // Subtle dither
    skyColor += vec3(dither);

    FragColor = vec4(skyColor, 1.0);
}
