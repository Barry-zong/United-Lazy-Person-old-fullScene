Shader "Custom/GlassEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.5
        _Transparency ("Transparency", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent"
            "Queue"="Transparent+100"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "URPGlassEffect"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _BlurAmount;
                float _Transparency;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 offset = float2(_BlurAmount, _BlurAmount) * 0.01;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // 采样场景颜色
                half4 sceneColor = half4(0, 0, 0, 1);
                sceneColor.rgb = SampleSceneColor(screenUV);
                
                // 简单的模糊效果
                half4 blurColor = half4(0, 0, 0, 1);
                blurColor.rgb += SampleSceneColor(screenUV + float2(offset.x, 0));
                blurColor.rgb += SampleSceneColor(screenUV - float2(offset.x, 0));
                blurColor.rgb += SampleSceneColor(screenUV + float2(0, offset.y));
                blurColor.rgb += SampleSceneColor(screenUV - float2(0, offset.y));
                blurColor.rgb = (blurColor.rgb + sceneColor.rgb) / 5.0;
                
                return lerp(blurColor, col, _Transparency);
            }
            ENDHLSL
        }
    }
} 