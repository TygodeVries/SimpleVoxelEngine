#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aUv;
layout(location = 2) in vec3 aNormal;

uniform mat4 u_Model;
uniform mat4 u_View;
uniform mat4 u_Projection;
uniform mat4 u_LightSpaceMatrix;

out vec2 Uv;
out vec3 Normal;
out vec3 WorldPosition;
out vec4 FragPosLightSpace;
out float Distance;

void main()
{
    vec4 worldPos = u_Model * vec4(aPosition, 1.0);
    vec4 viewPos = u_View * worldPos;

    WorldPosition = worldPos.xyz;
    Uv = aUv;
    Normal = mat3(u_Model) * aNormal; 
    FragPosLightSpace = u_LightSpaceMatrix * worldPos;

    Distance = length(viewPos.xyz); 

    gl_Position = u_Projection * viewPos;
}