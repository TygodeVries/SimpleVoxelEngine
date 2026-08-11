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
    
    if(projCoords.z > 1.0) return 0.0;

    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    float currentDepth = projCoords.z;
    
    // Normalize vectors for the dot product
    vec3 norm = normalize(Normal);
    vec3 sunDir = normalize(vec3(1.0, 1.0, 1.0)); 
    
    float bias = max(0.05 * (1.0 - dot(norm, sunDir)), 0.005);  
    
    float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;
    return shadow;
}

void main()
{   
    vec4 texColor = texture(u_Color, Uv);
    
    // Normalize normal for accurate lighting math
    vec3 norm = normalize(Normal);
    vec3 sunDir = normalize(vec3(0.4, 1.0, 0.4)); 
    
    // Simple diffuse lighting calculation
    float light = dot(norm, sunDir);
    light = (light + 1.0) / 2.0;
    if(light < 0.4) {
        light = 0.4;
    }
    
    float shadow = CalculateShadow(FragPosLightSpace);
    float shadowModifier = max(1.0 - shadow, 0.4);

    float fadeStart = 25.0;
    float fadeEnd = 30.0;

    if (Distance > fadeStart)
    {
        float fadeFactor = (Distance - fadeStart) / (fadeEnd - fadeStart);
        fadeFactor = clamp(fadeFactor, 0.0, 1.0);

        shadowModifier = mix(shadowModifier, 1.0, fadeFactor);
    }

    FragColor = texColor * light * shadowModifier;
}