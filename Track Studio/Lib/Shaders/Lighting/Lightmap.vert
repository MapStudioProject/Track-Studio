#version 330
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoords;

out vec2 texCoord;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
    texCoord = vec2(aTexCoords.x, 1.0 - aTexCoords.y);
}