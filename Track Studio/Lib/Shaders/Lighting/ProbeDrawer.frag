#version 330

in int instanceID;
in vec3 normal;

out vec4 fragOutput;

uniform mat4 mtxMdl;
uniform mat4 mtxCam;

vec3 calculateSH(
    vec3 normal,
    vec4 sh00,
    vec4 sh01,
    vec4 sh02,
    vec4 sh10,
    vec4 sh11,
    vec4 sh12,
    vec4 sh2)
{
    vec4 normal4 = vec4( normal.x, normal.y, normal.z, 1.0 );

    vec3 x0;
    x0.r         = dot( sh00, normal4 );
    x0.g         = dot( sh01, normal4 );
    x0.b         = dot( sh02, normal4 );

    vec4 v_b     = normal4.xyzz * normal4.yzzx;
    vec3 x1;
    x1.r         = dot( sh10, v_b );
    x1.g         = dot( sh11, v_b );
    x1.b         = dot( sh12, v_b );

    float v_c    = normal4.x * normal4.x - normal4.y * normal4.y;
    vec3  x2     = sh2.rgb * v_c;

    return max( ( x0 + x1 + x2 ), 0.0 );
}

layout (std140) uniform ProbeSHBuffer {
    vec4 probeBuffer[4096];
};

layout (std140) uniform ProbeInfo {
    vec4 probeInfo[1024];
};

void main(){

    int index = 7 * instanceID;

    vec3 color = calculateSH(normal,
                        probeBuffer[index],
                        probeBuffer[index+1],
                        probeBuffer[index+2],
                        probeBuffer[index+3],
                        probeBuffer[index+4],
                        probeBuffer[index+5],
                        probeBuffer[index+6]);

	fragOutput = vec4(color, 1.0);
}