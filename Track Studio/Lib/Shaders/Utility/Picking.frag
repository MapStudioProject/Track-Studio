#version 330

layout (location = 0) out vec4 fragOutput;

uniform vec4 color;

void main(){
    fragOutput = color;
}