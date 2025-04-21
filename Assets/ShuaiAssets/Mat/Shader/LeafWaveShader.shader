// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Universal Render Pipeline/Transparent/Cutout/Self Illum Diffuse Shake" {

    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Illum ("Illumin (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _ShakeDisplacement ("Displacement", Range (0, 1.0)) = 1.0
        _ShakeTime ("Shake Time", Range (0, 1.0)) = 1.0
        _ShakeWindspeed ("Shake Windspeed", Range (0, 1.0)) = 1.0
        _ShakeBending ("Shake Bending", Range (0, 1.0)) = 1.0
        
    }
    
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "VRCFallback" = "Hidden"
        }
        LOD 200
        
        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0
            
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float3 normalWS : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Illum);
            SAMPLER(sampler_Illum);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Illum_ST;
            float4 _Color;
            float _Cutoff;
            float _ShakeDisplacement;
            float _ShakeTime;
            float _ShakeWindspeed;
            float _ShakeBending;
            CBUFFER_END
            
            void FastSinCos (float4 val, out float4 s, out float4 c) {
                val = val * 6.408849 - 3.1415927;
                float4 r5 = val * val;
                float4 r6 = r5 * r5;
                float4 r7 = r6 * r5;
                float4 r8 = r6 * r5;
                float4 r1 = r5 * val;
                float4 r2 = r1 * r5;
                float4 r3 = r2 * r5;
                float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
                float4 cos8 = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};
                s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
                c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
            }
            
            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                
                float factor = (1 - _ShakeDisplacement - input.color.r) * 0.5;
                const float _WindSpeed = (_ShakeWindspeed + input.color.g);
                const float _WaveScale = _ShakeDisplacement;
                
                const float4 _waveXSize = float4(0.048, 0.06, 0.24, 0.096);
                const float4 _waveZSize = float4(0.024, .08, 0.08, 0.2);
                const float4 waveSpeed = float4(1.2, 2, 1.6, 4.8);
                
                float4 _waveXmove = float4(0.024, 0.04, -0.12, 0.096);
                float4 _waveZmove = float4(0.006, .02, -0.02, 0.1);
                
                float4 waves;
                waves = input.positionOS.x * _waveXSize;
                waves += input.positionOS.z * _waveZSize;
                
                waves += _Time.y * (1 - _ShakeTime * 2 - input.color.b) * waveSpeed * _WindSpeed;
                
                float4 s, c;
                waves = frac(waves);
                FastSinCos(waves, s, c);
                
                float waveAmount = input.uv.y * (input.color.a + _ShakeBending);
                s *= waveAmount;
                
                s *= normalize(waveSpeed);
                
                s = s * s;
                float fade = dot(s, 1.3);
                s = s * s;
                float3 waveMove = float3(0, 0, 0);
                waveMove.x = dot(s, _waveXmove);
                waveMove.z = dot(s, _waveZmove);
                
                input.positionOS.xz -= mul((float3x3)GetWorldToObjectMatrix(), waveMove).xz;
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uv2 = TRANSFORM_TEX(input.uv2, _Illum);
                output.color = input.color;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                half illum = SAMPLE_TEXTURE2D(_Illum, sampler_Illum, input.uv2).a;
                
                clip(col.a - _Cutoff);
                
                // 计算光照
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 diffuse = mainLight.color * NdotL;
                
                return half4((col.rgb + col.rgb * illum) * (diffuse + 0.5), col.a);
            }
            ENDHLSL
        }
        
        // 背面Pass
        Pass {
            Name "BackFace"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0
            
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float3 normalWS : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Illum);
            SAMPLER(sampler_Illum);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Illum_ST;
            float4 _Color;
            float _Cutoff;
            float _ShakeDisplacement;
            float _ShakeTime;
            float _ShakeWindspeed;
            float _ShakeBending;
            CBUFFER_END
            
            void FastSinCos (float4 val, out float4 s, out float4 c) {
                val = val * 6.408849 - 3.1415927;
                float4 r5 = val * val;
                float4 r6 = r5 * r5;
                float4 r7 = r6 * r5;
                float4 r8 = r6 * r5;
                float4 r1 = r5 * val;
                float4 r2 = r1 * r5;
                float4 r3 = r2 * r5;
                float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
                float4 cos8 = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};
                s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
                c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
            }
            
            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                
                float factor = (1 - _ShakeDisplacement - input.color.r) * 0.5;
                const float _WindSpeed = (_ShakeWindspeed + input.color.g);
                const float _WaveScale = _ShakeDisplacement;
                
                const float4 _waveXSize = float4(0.048, 0.06, 0.24, 0.096);
                const float4 _waveZSize = float4(0.024, .08, 0.08, 0.2);
                const float4 waveSpeed = float4(1.2, 2, 1.6, 4.8);
                
                float4 _waveXmove = float4(0.024, 0.04, -0.12, 0.096);
                float4 _waveZmove = float4(0.006, .02, -0.02, 0.1);
                
                float4 waves;
                waves = input.positionOS.x * _waveXSize;
                waves += input.positionOS.z * _waveZSize;
                
                waves += _Time.y * (1 - _ShakeTime * 2 - input.color.b) * waveSpeed * _WindSpeed;
                
                float4 s, c;
                waves = frac(waves);
                FastSinCos(waves, s, c);
                
                float waveAmount = input.uv.y * (input.color.a + _ShakeBending);
                s *= waveAmount;
                
                s *= normalize(waveSpeed);
                
                s = s * s;
                float fade = dot(s, 1.3);
                s = s * s;
                float3 waveMove = float3(0, 0, 0);
                waveMove.x = dot(s, _waveXmove);
                waveMove.z = dot(s, _waveZmove);
                
                input.positionOS.xz -= mul((float3x3)GetWorldToObjectMatrix(), waveMove).xz;
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uv2 = TRANSFORM_TEX(input.uv2, _Illum);
                output.color = input.color;
                output.normalWS = TransformObjectToWorldNormal(-input.normalOS); // 反转法线
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                half illum = SAMPLE_TEXTURE2D(_Illum, sampler_Illum, input.uv2).a;
                
                clip(col.a - _Cutoff);
                
                // 计算光照
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 diffuse = mainLight.color * NdotL;
                
                return half4((col.rgb + col.rgb * illum) * (diffuse + 0.5), col.a);
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Transparent/Cutout/VertexLit"
}