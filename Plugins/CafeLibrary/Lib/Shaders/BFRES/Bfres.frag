#version 330

in vec3 v_PositionWorld;
in vec2 v_TexCoord0;
in vec4 v_TexCoordBake;
in vec4 v_TexCoord23;
in vec4 v_VtxColor;
in vec3 v_NormalWorld;
in vec4 v_TangentWorld;

uniform int drawDebugAreaID;
uniform int areaID;
uniform float uBrightness;
uniform int isSelected;

uniform int displayVertexColors;

struct BakeResult {
    vec3 IndirectLight;
    float Shadow;
    float AO;
};
struct LightResult {
    vec3 DiffuseColor;
    vec3 SpecularColor;
};
struct DirectionalLight {
    vec3 Color;
    vec3 BacksideColor;
    vec3 Direction;
    bool Wrapped;
    bool VisibleInShadow;
};
struct SurfaceLightParams {
    vec3 SurfaceNormal;
    vec3 SurfacePointToEyeDir;
    vec3 SpecularColor;
    float IntensityFromShadow;
    float SpecularRoughness;
};
float G1V(float NoV, float k) {
    return 1.0 / (NoV * (1.0 - k) + k);
}

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

//Samplers
uniform sampler2D u_TextureAlbedo0;   // _a0
uniform sampler2D u_TextureSpecMask;  // _s0
uniform sampler2D u_TextureNormal0;   // _n0
uniform sampler2D u_TextureNormal1;   // _n1
uniform sampler2D u_TextureEmission0; // _e0
uniform sampler2D u_TextureBake0;     // _b0
uniform sampler2D u_TextureBake1;     // _b1
uniform sampler2D u_TextureMultiA;    // _a1
uniform sampler2D u_TextureMultiB;    // _a2
uniform sampler2D u_TextureIndirect;  // _a3

#define enable_diffuse
#define enable_diffuse2
#define enable_albedo
#define enable_emission
#define enable_emission_map
#define enable_specular
#define enable_specular_mask
#define enable_specular_mask_rougness
#define enable_specular_physical
#define enable_vtx_color_diff
#define enable_vtx_color_emission
#define enable_vtx_color_spec
#define enable_vtx_alpha_trans

uniform bool alphaTest;
uniform int alphaFunc;
uniform float alphaRefValue;

//Toggles
uniform int hasDiffuseMap;

//GL
uniform mat4 mtxCam;
uniform int colorOverride;
uniform vec4 highlight_color;


out vec4 fragOutput;

float GetComponent(int Type, vec4 Texture);

void main(){

    if (colorOverride == 1)
    {
        fragOutput = vec4(1);
        return;
    }

    vec3 N = v_NormalWorld;
    vec3 displayNormal = (N.xyz * 0.5) + 0.5;

    vec4 diffuseMapColor = vec4(1);
    vec2 texCoord0 = v_TexCoord0;

    if (hasDiffuseMap == 1) {
        diffuseMapColor = texture(u_TextureAlbedo0,texCoord0);
    }

    float halfLambert = max(displayNormal.y,0.5);
    fragOutput = vec4(diffuseMapColor.rgb * halfLambert, diffuseMapColor.a);
    fragOutput.rgb *= vec3(uBrightness);

    //Alpha test
    if (alphaTest)
    {
        switch (alphaFunc)
        {
            case 0: //gequal
                if (fragOutput.a <= alphaRefValue)
                {
                     discard;
                }
            break;
            case 1: //greater
                if (fragOutput.a < alphaRefValue)
                {
                     discard;
                }
            break;
            case 2: //equal
                if (fragOutput.a == alphaRefValue)
                {
                     discard;
                }
            break;
            case 3: //less
                if (fragOutput.a > alphaRefValue)
                {
                     discard;
                }
            break;
            case 4: //lequal
                if (fragOutput.a >= alphaRefValue)
                {
                     discard;
                }
            break;
        }
    }


	if (displayVertexColors == 1)
		fragOutput.rgba *= v_VtxColor.rgba;

    if (drawDebugAreaID == 1)
    {
         vec3 areaOverlay = vec3(0);
         if (areaID == 0) areaOverlay = vec3(1, 0, 0);
         if (areaID == 1) areaOverlay = vec3(0, 1, 0);
         if (areaID == 2) areaOverlay = vec3(0, 0, 1);
         if (areaID == 3) areaOverlay = vec3(1, 1, 0);
         if (areaID == 4) areaOverlay = vec3(0, 1, 1);
         if (areaID == 5) areaOverlay = vec3(1, 0, 1);

         fragOutput.rgb = mix(fragOutput.rgb, areaOverlay, 0.5);
    }
}