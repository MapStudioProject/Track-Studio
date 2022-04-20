#version 330
layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec4 vCoef0;
layout (location = 2) in vec4 vCoef1;
layout (location = 3) in vec4 vCoef2;
layout (location = 4) in vec4 vCoef3;
layout (location = 5) in vec4 vCoef4;
layout (location = 6) in vec4 vCoef5;
layout (location = 7) in vec4 vCoef6;

out vec3 color;

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


void main(){    
	gl_Position = mtxCam * vec4(vPosition.xyz, 1);
    color = calculateSH(vec3(1.0),
                        vCoef0,
                        vCoef1,
                        vCoef2,
                        vCoef3,
                        vCoef4,
                        vCoef5,
                        vCoef6);
}