Shader "Custom/SimpleGrabPassBlurURP" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _BumpAmt  ("Distortion", Range (0,128)) = 10
        _MainTex ("Tint Color (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _Size ("Size", Range(0, 20)) = 1
    }

    SubShader {
        Tags { 
            "Queue"="Transparent+1" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Horizontal blur
        Pass {
            Name "Horizontal Blur"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _Size;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float4 sum = float4(0,0,0,0);
                #define SAMPLE(weight, offset) SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(_MainTex_TexelSize.x * offset * _Size, 0)) * weight
                
                sum += SAMPLE(0.05, -4.0);
                sum += SAMPLE(0.09, -3.0);
                sum += SAMPLE(0.12, -2.0);
                sum += SAMPLE(0.15, -1.0);
                sum += SAMPLE(0.18,  0.0);
                sum += SAMPLE(0.15, +1.0);
                sum += SAMPLE(0.12, +2.0);
                sum += SAMPLE(0.09, +3.0);
                sum += SAMPLE(0.05, +4.0);

                return sum;
            }
            ENDHLSL
        }

        // Vertical blur
        Pass {
            Name "Vertical Blur"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _Size;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float4 sum = float4(0,0,0,0);
                #define SAMPLE(weight, offset) SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, _MainTex_TexelSize.y * offset * _Size)) * weight
                
                sum += SAMPLE(0.05, -4.0);
                sum += SAMPLE(0.09, -3.0);
                sum += SAMPLE(0.12, -2.0);
                sum += SAMPLE(0.15, -1.0);
                sum += SAMPLE(0.18,  0.0);
                sum += SAMPLE(0.15, +1.0);
                sum += SAMPLE(0.12, +2.0);
                sum += SAMPLE(0.09, +3.0);
                sum += SAMPLE(0.05, +4.0);

                return sum;
            }
            ENDHLSL
        }

        // Distortion
        Pass {
            Name "Distortion"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvBump : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            float4 _MainTex_TexelSize;
            float _BumpAmt;
            float4 _Color;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.uvBump = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float2 bump = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uvBump)).rg;
                float2 offset = bump * _BumpAmt * _MainTex_TexelSize.xy;
                
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset);
                float4 tint = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;

                return col * tint;
            }
            ENDHLSL
        }
    }
} 