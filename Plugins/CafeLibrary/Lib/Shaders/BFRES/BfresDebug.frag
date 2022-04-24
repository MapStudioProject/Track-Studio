#version 330

uniform sampler2D DiffuseTexture;
uniform sampler2D UVTestPattern;
uniform sampler2D EmissionTexture;
uniform sampler2D SpecularTexture;
uniform sampler2D NormalMapTexture;
uniform sampler2D AmbientOccusionBakeTexture;
uniform sampler2D ShadowBakeTexture;
uniform sampler2D IndirectLightBakeTexture;

uniform int debugShading;
uniform vec4 highlight_color;
uniform vec3 light_bake_scale;
uniform vec4 color;

uniform int DrawAreaID;
uniform int AreaIndex;
uniform int isSelected;
uniform int hasProbes;
uniform int isNormalMapBC1;

in vec2 texCoord0;
in vec3 normal;
in vec3 boneWeightsColored;
in vec3 tangent;
in vec3 bitangent;
in vec4 vertexColor;
in vec2 texCoordBake0;
in vec2 texCoordBake1;

in vec3 probeLight;

out vec4 fragOutput;
out vec4 selectionMask;

const int DISPLAY_NORMALS = 1;
const int DISPLAY_LIGHTING = 2;
const int DISPLAY_DIFFUSE = 3;
const int DISPLAY_VTX_CLR = 4;
const int DISPLAY_UV = 5;
const int DISPLAY_UV_PATTERN = 6;
const int DISPLAY_WEIGHTS = 7;
const int DISPLAY_TANGENT = 8;
const int DISPLAY_BITANGENT = 9;

const int DISPLAY_SPECULAR = 10;
const int DISPLAY_NORMAL = 11;
const int DISPLAY_EMISSION = 12;
const int DISPLAY_AO = 13;
const int DISPLAY_SHADOW = 14;
const int DISPLAY_LIGHTMAP = 15;
const int DISPLAY_LIGHTMAP_AMOUNT = 16;

float saturate(float v) { return clamp(v, 0.0, 1.0); }
vec2 saturate(vec2 v) { return clamp(v, vec2(0.0), vec2(1.0)); }
vec3 saturate(vec3 v) { return clamp(v, vec3(0.0), vec3(1.0)); }
vec4 saturate(vec4 v) { return clamp(v, vec4(0.0), vec4(1.0)); }

vec3 ReconstructNormal(in vec2 t_NormalXY) {
    float t_NormalZ = sqrt(saturate(1.0 - dot(t_NormalXY.xy, t_NormalXY.xy)));
    return vec3(t_NormalXY.xy, t_NormalZ);
}

void main(){
    vec4 outputColor = vec4(1);
    vec2 displayTexCoord = texCoord0;
    selectionMask = vec4(0);
    if (isSelected == 1)
        selectionMask = vec4(1.0);

    vec3 N = normal;

    if (debugShading == DISPLAY_NORMALS)
    {
        vec3 displayNormal = (N * 0.5) + 0.5;
        outputColor.rgb = displayNormal;
    }
    if (debugShading == DISPLAY_DIFFUSE)
    {
        outputColor = texture(DiffuseTexture,displayTexCoord);
    }
    if (debugShading == DISPLAY_LIGHTING)
    {
        vec3 displayNormal = (N * 0.5) + 0.5;
        float halfLambert = max(displayNormal.y,0.5);
        outputColor.rgb = vec3(0.5) * halfLambert;
    }
    if (debugShading == DISPLAY_UV)
         outputColor.rgb = vec3(displayTexCoord.x, displayTexCoord.y, 1.0);
    if (debugShading == DISPLAY_UV_PATTERN)
        outputColor.rgb = texture(UVTestPattern, displayTexCoord).rgb;
    if (debugShading == DISPLAY_WEIGHTS)
        outputColor.rgb = boneWeightsColored;
    if (debugShading == DISPLAY_TANGENT)
    {
        vec3 displayTangent = (tangent * 0.5) + 0.5;
        outputColor.rgb = displayTangent;
    }
    if (debugShading == DISPLAY_BITANGENT)
    {
        vec3 displayBitangent = (bitangent * 0.5) + 0.5;
        outputColor.rgb = displayBitangent;
    }
    if (debugShading == DISPLAY_VTX_CLR)
    {
        outputColor.rgb = vertexColor.rgb;
    }

    if (debugShading == DISPLAY_SPECULAR)
        outputColor = texture(SpecularTexture,displayTexCoord);
    if (debugShading == DISPLAY_NORMAL)
    {
        vec2 normalMap = texture(NormalMapTexture,displayTexCoord).xy;
        //BC5 Snorm conversion
        if (isNormalMapBC1 == 0.0)
           normalMap = (normalMap + 1.0) / 2.0;

        outputColor.rg = normalMap;
        outputColor.b = 1.0;
    }
    if (debugShading == DISPLAY_EMISSION)
        outputColor = texture(EmissionTexture,displayTexCoord);
    if (debugShading == DISPLAY_AO)
        outputColor.rgb = texture(AmbientOccusionBakeTexture,texCoordBake0).rrr;
    if (debugShading == DISPLAY_SHADOW)
        outputColor.rgb = texture(ShadowBakeTexture,texCoordBake0).ggg;
    if (debugShading == DISPLAY_LIGHTMAP)
    {
        vec4 light_tex = texture(IndirectLightBakeTexture,texCoordBake1);
        outputColor.rgb = light_bake_scale * light_tex.rgb * light_tex.a;

        if (hasProbes == 1)
            outputColor.rgb += probeLight;
    }   
    if (debugShading == DISPLAY_LIGHTMAP_AMOUNT)
        outputColor.rgb = texture(IndirectLightBakeTexture,texCoordBake1).aaa;

    fragOutput = outputColor;

	if (highlight_color.w > 0.0)
	{
		//Highlight intensity for object selection
		float hc_a   = highlight_color.w;
		fragOutput = vec4(fragOutput.rgb * (1-hc_a) + highlight_color.rgb * hc_a, fragOutput.a);
	}

    if (DrawAreaID == 1)
    {
         if (AreaIndex == 0) fragOutput = vec4(1, 0, 0, 1);
         if (AreaIndex == 1) fragOutput = vec4(0, 1, 0, 1);
         if (AreaIndex == 2) fragOutput = vec4(0, 0, 1, 1);
         if (AreaIndex == 3) fragOutput = vec4(1, 1, 0, 1);
         if (AreaIndex == 4) fragOutput = vec4(0, 1, 1, 1);
         if (AreaIndex == 5) fragOutput = vec4(1, 0, 1, 1);
    }
}