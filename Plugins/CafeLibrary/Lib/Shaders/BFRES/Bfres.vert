#version 450 core

#define SKIN_COUNT 16

precision mediump float;

struct EnvLightParam {
    vec4 BacksideColor;
    vec4 DiffuseColor;
    vec4 Direction;
};
layout(std140) uniform ub_MaterialParams {
    mat3x4 u_TexCoordSRT0;
    vec4 u_TexCoordBake0ScaleBias;
    vec4 u_TexCoordBake1ScaleBias;
    mat3x4 u_TexCoordSRT2;
    mat3x4 u_TexCoordSRT3;
    vec4 u_AlbedoColorAndTransparency;
    vec4 u_EmissionColorAndNormalMapWeight;
    vec4 u_SpecularColorAndIntensity;
    vec4 u_BakeLightScaleAndRoughness;
    vec4 u_MultiTexReg[3];
    vec4 u_Misc[1];
    EnvLightParam u_EnvLightParams[2];
};

layout(std430, binding = 3) buffer GsysSkeleton {
    mat4 cBoneMatrices[];
};

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vTexCoord0;
layout (location = 3) in vec2 vTexCoord1;
layout (location = 4) in vec2 vTexCoord2;
layout (location = 5) in vec4 vColor;
layout (location = 6) in ivec4 vBoneIndex;
layout (location = 7) in vec4 vBoneWeight;
layout (location = 8) in vec4 vTangent;
layout (location = 9) in vec4 vBitangent;

layout (location = 10) in ivec4 vBoneIndex2;
layout (location = 11) in vec4 vBoneWeight2;
layout (location = 12) in ivec4 vBoneIndex3;
layout (location = 13) in vec4 vBoneWeight3;
layout (location = 14) in ivec4 vBoneIndex4;
layout (location = 15) in vec4 vBoneWeight4;

uniform mat4 mtxMdl;
uniform mat4 mtxCam;

// Skinning uniforms
uniform int SkinCount;
uniform int UseSkinning;
uniform int BoneIndex;
uniform mat4 RigidBindTransform;

out vec3 v_PositionWorld;
out vec2 v_TexCoord0;
out vec4 v_TexCoordBake;
out vec4 v_TexCoord23;
out vec4 v_VtxColor;
out vec3 v_NormalWorld;
out vec4 v_TangentWorld;

vec2 CalcScaleBias(in vec2 t_Pos, in vec4 t_SB) {
    return t_Pos.xy * t_SB.xy + t_SB.zw;
}

vec4 skin(vec3 pos)
{
    vec4 newPosition = vec4(pos.xyz, 1.0);

    if (SkinCount == 1) //Rigid
    {
        newPosition = mat4(cBoneMatrices[vBoneIndex.x]) * vec4(pos, 1.0);
    }
    else //Smooth
    {
	    if (SKIN_COUNT >= 1) newPosition =  vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex.x]) * vBoneWeight.x;
	    if (SKIN_COUNT >= 2) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex.y]) * vBoneWeight.y;
	    if (SKIN_COUNT >= 3) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex.z]) * vBoneWeight.z;
	    if (SKIN_COUNT >= 4 && vBoneWeight.w < 1) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex.w]) * vBoneWeight.w;
	    if (SKIN_COUNT >= 5) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex2.x]) * vBoneWeight2.x;
	    if (SKIN_COUNT >= 6) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex2.y]) * vBoneWeight2.y;
	    if (SKIN_COUNT >= 7) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex2.z]) * vBoneWeight2.z;
	    if (SKIN_COUNT >= 8 && vBoneWeight2.w < 1) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex2.w]) * vBoneWeight2.w;
	    if (SKIN_COUNT >= 9) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex3.x]) * vBoneWeight3.x;
	    if (SKIN_COUNT >= 10) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex3.y]) * vBoneWeight3.y;
	    if (SKIN_COUNT >= 11) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex3.z]) * vBoneWeight3.z;
	    if (SKIN_COUNT >= 12 && vBoneWeight3.w < 1) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex3.w]) * vBoneWeight3.w;
	    if (SKIN_COUNT >= 13) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex4.x]) * vBoneWeight4.x;
	    if (SKIN_COUNT >= 14) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex4.y]) * vBoneWeight4.y;
	    if (SKIN_COUNT >= 15) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex4.z]) * vBoneWeight4.z;
	    if (SKIN_COUNT >= 16 && vBoneWeight4.w < 1) newPosition += vec4(pos, 1.0) * mat4(cBoneMatrices[vBoneIndex4.w]) * vBoneWeight4.w;
    }

    return newPosition;
}

vec3 skinNRM(vec3 nr)
{
    vec3 newNormal = vec3(0);

    if (SkinCount == 1) //Rigid
    {
        newNormal = mat3(cBoneMatrices[vBoneIndex.x]) * nr;
    }
    else //Smooth
    {
	    if (SKIN_COUNT >= 1) newNormal =  nr * mat3(cBoneMatrices[vBoneIndex.x]) * vBoneWeight.x;
	    if (SKIN_COUNT >= 2) newNormal += nr * mat3(cBoneMatrices[vBoneIndex.y]) * vBoneWeight.y;
	    if (SKIN_COUNT >= 3) newNormal += nr * mat3(cBoneMatrices[vBoneIndex.z]) * vBoneWeight.z;
	    if (SKIN_COUNT >= 4) newNormal += nr * mat3(cBoneMatrices[vBoneIndex.w]) * vBoneWeight.w;
	    if (SKIN_COUNT >= 5) newNormal += nr * mat3(cBoneMatrices[vBoneIndex2.x]) * vBoneWeight2.x;
	    if (SKIN_COUNT >= 6) newNormal += nr * mat3(cBoneMatrices[vBoneIndex2.y]) * vBoneWeight2.y;
	    if (SKIN_COUNT >= 7) newNormal += nr * mat3(cBoneMatrices[vBoneIndex2.z]) * vBoneWeight2.z;
	    if (SKIN_COUNT >= 8) newNormal += nr * mat3(cBoneMatrices[vBoneIndex2.w]) * vBoneWeight2.w;
	    if (SKIN_COUNT >= 9) newNormal += nr * mat3(cBoneMatrices[vBoneIndex3.x]) * vBoneWeight3.x;
	    if (SKIN_COUNT >= 10) newNormal += nr * mat3(cBoneMatrices[vBoneIndex3.y]) * vBoneWeight3.y;
	    if (SKIN_COUNT >= 11) newNormal += nr * mat3(cBoneMatrices[vBoneIndex3.z]) * vBoneWeight3.z;
	    if (SKIN_COUNT >= 12) newNormal += nr * mat3(cBoneMatrices[vBoneIndex3.w]) * vBoneWeight3.w;
	    if (SKIN_COUNT >= 13) newNormal += nr * mat3(cBoneMatrices[vBoneIndex4.x]) * vBoneWeight4.x;
	    if (SKIN_COUNT >= 14) newNormal += nr * mat3(cBoneMatrices[vBoneIndex4.y]) * vBoneWeight4.y;
	    if (SKIN_COUNT >= 15) newNormal += nr * mat3(cBoneMatrices[vBoneIndex4.z]) * vBoneWeight4.z;
	    if (SKIN_COUNT >= 16) newNormal += nr * mat3(cBoneMatrices[vBoneIndex4.w]) * vBoneWeight4.w;
    }

    return newNormal;
}

void main(){
    vec4 worldPosition = vec4(vPosition.xyz, 1);
    vec3 normal = normalize(mat3(mtxMdl) * vNormal.xyz);

    //Vertex Rigging
    if (UseSkinning == 1) //Animated object using the skeleton
    {
        ivec4 index = vBoneIndex;

        //Apply skinning to vertex position and normal
	    if (SkinCount > 0)
		    worldPosition = skin(worldPosition.xyz);
	    if(SkinCount > 0)
		    normal = normalize(mat3(mtxMdl) * (skinNRM(vNormal.xyz)).xyz);
        //Single bind models that have no skinning to the bone they are mapped to
        if (SkinCount == 0)
        {
            worldPosition = RigidBindTransform * worldPosition;
            normal = mat3(RigidBindTransform) * normal;
        }
    }

    vec3 fragPosition = (mtxMdl * worldPosition).xyz;
    gl_Position = mtxCam * vec4(fragPosition, 1);

    v_PositionWorld = fragPosition.xyz;
  //  v_TexCoord0 = mat4x2(u_TexCoordSRT0) * vec4(vTexCoord0.xy, 1.0, 1.0);
    v_TexCoord0 = vTexCoord0.xy;
    v_TexCoordBake.xy = CalcScaleBias(vTexCoord1.xy, u_TexCoordBake0ScaleBias);
    v_TexCoordBake.zw = CalcScaleBias(vTexCoord1.xy, u_TexCoordBake1ScaleBias);
    v_TexCoord23.xy = mat4x2(u_TexCoordSRT2) * vec4(vTexCoord2.xy, 1.0, 1.0);
    //v_TexCoord23.zw = mat4x2(u_TexCoordSRT3) * vec4(vTexCoord3.xy, 1.0, 1.0);
    v_VtxColor = vColor;
    v_NormalWorld.xyz = normalize(normal.xyz);
    v_TangentWorld.xyzw = vTangent.xyzw;
}