#version 330 core

in float Distance;
in vec2 Uv;
in vec3 Normal;
in vec4 FragPosLightSpace;

out vec4 FragColor;

uniform sampler2D u_Color;
// u_TextureInfo.x = total atlas columns (BlockTexturesMap.row)
// u_TextureInfo.y = total atlas rows (BlockTexturesMap.col)
// u_TextureInfo.z = mesh scale multiplier (16.0)
uniform vec4 u_TextureInfo; 

void main()
{   
    // 1. Calculate UV Tiling Math
    float cols = u_TextureInfo.x;
    float rows = u_TextureInfo.y;
    float meshScale = u_TextureInfo.z;

    vec2 scaledUv = Uv * meshScale;
    vec2 tileSize = vec2(1.0 / cols, 1.0 / rows);
    vec2 tileOrigin = floor(Uv * vec2(cols, rows)) * tileSize;
    vec2 wrappedUv = tileOrigin + mod(scaledUv, tileSize);

    // 2. Fetch Texture
    vec4 texColor = texture(u_Color, wrappedUv);

    // 3. Match Standard Shader Lighting (Without Shadows)
    vec3 norm = normalize(Normal);
    vec3 sunDir = normalize(vec3(0.4, 1.0, 0.4)); 

    float light = dot(norm, sunDir);
    light = (light + 1.0) / 2.0;
    if (light < 0.4) {
        light = 0.4;
    }

    FragColor = texColor * light;
}