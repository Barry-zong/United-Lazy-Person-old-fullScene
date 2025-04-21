#ifndef UNIVERSAL_LEAF_WAVE_INPUT_INCLUDED
#define UNIVERSAL_LEAF_WAVE_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Cutoff;
float _Surface;
float _ShakeDisplacement;
float _ShakeTime;
float _ShakeWindspeed;
float _ShakeBending;
CBUFFER_END

#endif 