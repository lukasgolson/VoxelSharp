#version 330 core
in vec3 skyDirection; // Interpolated direction from vertex shader
out vec4 FragColor;

uniform float time;
// 0.0 = midnight, 0.5 = noon, 1.0 = next midnight

// Gradient function to create smooth transitions
vec3 gradientSkyColor(float height, float time) {
    // --- Define Key Colors (RGB) ---
    vec3 nightZenith = vec3(0.02, 0.02, 0.07); // Darker Blue/Purple for night sky (top)
    vec3 nightHorizon = vec3(0.05, 0.05, 0.15); // Night horizon
    vec3 dayZenith = vec3(0.4, 0.7, 1.0);     // Bright blue (day top)
    vec3 dayHorizon = vec3(0.7, 0.85, 1.0);   // Light blue (day bottom)
    vec3 warmHorizon = vec3(1.0, 0.3, 0.0);   // Sunset orange/red

    // --- Calculate Time Factors ---
    float rotation = time * 2.0 * 3.14159; // Full rotation over 1 day

    // 1. Daytime Intensity Factor (sin(angle) shifted): Goes from 0 (midnight) to 1 (noon) and back to 0.
    // We use a cosine function offset by 90 degrees (1.570796 radians) to map time=0.5 to peak intensity.
    float sunY = sin(rotation - 1.570796);
    float dayIntensity = pow(clamp(sunY * 0.5 + 0.5, 0.0, 1.0), 3.0); // Use a power curve for smoother dimming

    // 2. Horizon Glow Factor (Peaks sharply at sunrise/sunset, i.e., when sunY is near 0)
    // This finds how close 'sunY' is to the horizon (0.0).
    float distanceToHorizon = 1.0 - abs(sunY);
    // Use a high power curve to create a sharp glow that quickly fades as the sun rises/sets.
    float glowFactor = pow(distanceToHorizon, 8.0);

    // --- Interpolating Colors (Gradient Logic) ---

    // 1. Interpolate Zenith Color: Night to Day (top of the sky)
    vec3 topColor = mix(nightZenith, dayZenith, dayIntensity);

    // 2. Interpolate Horizon Color: Night to Day
    vec3 midColor = mix(nightHorizon, dayHorizon, dayIntensity);

    // 3. Blend in the Warm Color at the horizon during the glow period.
    midColor = mix(midColor, warmHorizon, glowFactor);

    // 4. Mix Vertical Gradient: Blend from the horizon color (bottom) to the zenith color (top)
    return mix(midColor, topColor, smoothstep(0.0, 1.0, height));
}

// Random function for dithering
float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453);
}

// Main function
void main()
{
    // Normalize sky direction to get an up-down factor
    float heightFactor = normalize(skyDirection).y;
    heightFactor = (heightFactor + 1.0) * 0.5; // Convert range to [0,1]

    // Get the continuous sky color using the gradient function with time rotation
    vec3 skyColor = gradientSkyColor(heightFactor, time);

    // Subtle dithering
    float dither = (random(gl_FragCoord.xy * 0.1) - 0.5) * 0.01;
    skyColor += vec3(dither);

    FragColor = vec4(skyColor, 1.0);
}