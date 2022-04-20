#version 330

uniform sampler2D uvTexture;

in vec2 TexCoords;

uniform float brightness;
uniform int isSRGB;
uniform int hasTexture;
uniform vec4 uColor;

out vec4 fragOutput;

void main()
{  
    vec4 color = uColor;
    if (hasTexture == 1)
         color *= texture(uvTexture, TexCoords);
    fragOutput = vec4(color.rgb, 1.0);

    if (isSRGB == 1)
        fragOutput.rgb = pow(fragOutput.rgb, vec3(1.0/2.2));

    fragOutput.rgb *= brightness;
}  