#version 330 core

in float Distance;
in vec2 Uv;
in vec3 Normal;
in vec4 FragPosLightSpace;

out vec4 FragColor;

uniform sampler2D u_Color;
uniform sampler2D shadowMap;

float CalculateShadow(vec4 fragPosLightSpace) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    
    if (projCoords.z > 1.0)
        return 0.0;

    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    
    vec3 norm = normalize(Normal);
    vec3 sunDir = normalize(vec3(1.0, 1.0, 1.0)); 
    
    float bias = max(0.05 * (1.0 - dot(norm, sunDir)), 0.005);
    
    return currentDepth - bias > closestDepth ? 1.0 : 0.0;
}

void main()
{
    vec4 texColor = texture(u_Color, Uv);

    vec3 norm = normalize(Normal);
    vec3 sunDir = normalize(vec3(0.4, 1.0, 0.4));

    float light = dot(norm, sunDir);
    light = (light + 1.0) / 2.0;
    light = max(light, 0.4);

    float shadow = CalculateShadow(FragPosLightSpace);
    float shadowModifier = max(1.0 - shadow, 0.4);

    // Fade shadows with distance
    float shadowFadeStart = 25.0;
    float shadowFadeEnd = 30.0;

    if (Distance > shadowFadeStart)
    {
        float fadeFactor = (Distance - shadowFadeStart) /
                           (shadowFadeEnd - shadowFadeStart);

        fadeFactor = clamp(fadeFactor, 0.0, 1.0);
        shadowModifier = mix(shadowModifier, 1.0, fadeFactor);
    }

    // Lighting + shadow
    vec4 color = texColor * light * shadowModifier;
	
    float fogStart = 20.0;
    float fogEnd = 80.0;

    float fogFactor = (Distance - fogStart) /
                      (fogEnd - fogStart);

    fogFactor = clamp(fogFactor, 0.0, 1.0);

    // Fog color
    vec3 fogColor = vec3(0.65, 0.72, 0.80);

    // Blend scene into fog
    color.rgb = mix(color.rgb, fogColor, fogFactor);

    FragColor = color;
}