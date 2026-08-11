#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 2) in vec2 aUv;

uniform mat4 u_Model;
uniform mat4 u_Projection;

out vec2 Uv;

void main()
{
    Uv = aUv;
    gl_Position = u_Projection * u_Model * vec4(aPosition, 1.0);
}