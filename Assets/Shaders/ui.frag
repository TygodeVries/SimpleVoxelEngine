#version 330 core

in float Distance;
in vec2 Uv;

out vec4 FragColor;

uniform sampler2D u_Color;

void main()
{   
    vec4 texColor = texture(u_Color, Uv);
    if(texColor.a < 0.1)
        discard;
    FragColor = texColor;
}