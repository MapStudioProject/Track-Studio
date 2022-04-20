#version 330
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoords;

out vec4 in_attr0;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
    in_attr0 = vec4(aTexCoords.x, 1.0 - aTexCoords.y, 0, 0);
}