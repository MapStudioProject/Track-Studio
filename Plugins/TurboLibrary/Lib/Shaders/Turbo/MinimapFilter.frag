#version 330

in vec2 TexCoords;
uniform vec3 color;

out vec4 fragOutput;

uniform sampler2D normalsTexture;

void main(){
    vec3 normals = texture(normalsTexture, TexCoords).rgb;
    float alpha = texture(normalsTexture, TexCoords).a;

    vec3 displayNormal = (normals.xyz * 0.5) + 0.5;
	float shading = max(displayNormal.y,0.5);
    fragOutput = vec4(color * vec3(shading), alpha);
}