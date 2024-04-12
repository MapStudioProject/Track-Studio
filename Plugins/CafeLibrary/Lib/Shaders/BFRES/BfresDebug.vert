#version 450 core

#define SKIN_COUNT 16

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord0;
in vec2 vTexCoord1;
in vec2 vTexCoord2;
in vec4 vColor;
in ivec4 vBoneIndex;
in vec4 vBoneWeight;
in vec3 vTangent;
in vec3 vBitangent;

in ivec4 vBoneIndex2;
in vec4 vBoneWeight2;
in ivec4 vBoneIndex3;
in vec4 vBoneWeight3;
in ivec4 vBoneIndex4;
in vec4 vBoneWeight4;

layout(std430, binding = 3) buffer GsysSkeleton {
    mat4 cBoneMatrices[];
};

uniform mat4 mtxMdl;
uniform mat4 mtxCam;
uniform mat4 mtxLightVP;

// Skinning uniforms
uniform int SkinCount;
uniform int UseSkinning;
uniform int BoneIndex;
uniform mat4 RigidBindTransform;

uniform sampler2D weightRamp1;
uniform sampler2D weightRamp2;
uniform int selectedBoneIndex;
uniform int weightRampType;

out vec2 texCoord0;
out vec3 normal;
out vec3 boneWeightsColored;
out vec3 tangent;
out vec3 bitangent;
out vec4 vertexColor;
out vec4 fragPosLightSpace;
out vec3 probeLight;
out vec2 texCoordBake0;
out vec2 texCoordBake1;

uniform int hasProbes;
uniform vec4 probeSH[7];
uniform vec4 bake0_st;
uniform vec4 bake1_st;

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
    return vec3(0.0);
}


vec4 skin(vec3 pos)
{
    vec4 newPosition = vec4(pos.xyz, 1.0);

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

    return newPosition;
}

vec3 skinNormal(vec3 nr)
{
    vec3 newNormal = vec3(0);

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

    return newNormal;
}

vec3 BoneWeightColor(float weights)
{
	float rampInputLuminance = weights;
	rampInputLuminance = clamp((rampInputLuminance), 0.001, 0.999);
    if (weightRampType == 1) // Greyscale
        return vec3(weights);
    else if (weightRampType == 2) // Color 1
	   return texture(weightRamp1, vec2(1 - rampInputLuminance, 0.50)).rgb;
    else // Color 2
        return texture(weightRamp2, vec2(1 - rampInputLuminance, 0.50)).rgb;
}

float BoneWeightDisplay(ivec4 index)
{
    float weight = 0;
    if (selectedBoneIndex == index.x)
        weight += vBoneWeight.x;
    if (selectedBoneIndex == index.y)
        weight += vBoneWeight.y;
    if (selectedBoneIndex == index.z)
        weight += vBoneWeight.z;
    if (selectedBoneIndex == index.w)
        weight += vBoneWeight.w;

    if (selectedBoneIndex == index.x && SkinCount == 1)
        weight = 1;
   if (selectedBoneIndex == BoneIndex && SkinCount == 0)
        weight = 1;

    return weight;
}

void main(){
    vec4 worldPosition = vec4(vPosition.xyz, 1);
    normal = normalize(mat3(mtxMdl) * vNormal.xyz);

    //Vertex Rigging
    if (UseSkinning == 1) //Animated object using the skeleton
    {
        ivec4 index = vBoneIndex;
        //Apply skinning to vertex position and normal
	    if (SkinCount > 0)
		    worldPosition = skin(worldPosition.xyz);
	    if (SkinCount > 0)
		    normal = skinNormal(normal.xyz);
        //Single bind models that have no skinning to the bone they are mapped to
        if (SkinCount == 0)
        {
            worldPosition = RigidBindTransform * worldPosition;
            normal = mat3(RigidBindTransform) * normal;
        }
    }

    vec3 fragPosition = (mtxMdl * worldPosition).xyz;
    gl_Position = mtxCam*vec4(fragPosition, 1);

    float totalWeight = BoneWeightDisplay(vBoneIndex);
    boneWeightsColored = BoneWeightColor(totalWeight).rgb;
    texCoord0 = vTexCoord0;
    tangent = vTangent;
    bitangent = vBitangent;
    vertexColor = vColor;
    fragPosLightSpace = mtxLightVP * vec4(fragPosition, 1.0);
    texCoordBake0 = vTexCoord1 * bake0_st.xy + bake0_st.zw;
    texCoordBake1 = vTexCoord1 * bake1_st.xy + bake1_st.zw;

    probeLight = vec3(0);
    if (hasProbes == 1)
    {
        probeLight = calculateSH(normal, 
            probeSH[0], probeSH[1], probeSH[2], 
            probeSH[3], probeSH[4], probeSH[5], probeSH[6]);
    }
}