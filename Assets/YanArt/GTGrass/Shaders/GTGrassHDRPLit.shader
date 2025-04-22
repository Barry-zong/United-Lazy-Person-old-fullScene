Shader "YanArt/GTGrassHDRPLit"
{
    Properties
    {
        _BaseColorMap("BaseMap", 2D) = "white" {}
        _AlphaClip("AlphaClip", Range(0,1)) = 0.5
        _BaseColor("BaseColor", Color) = (1,1,1,1)
        [HDR]_BottomColor("BottomColor", Color) = (1,1,1,1)
        [HDR]_TopColor("TopColor", Color) = (1,1,1,1)
        _Power("Color Power", float) = 1
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.5

        [Header(Blade Diversity)]
        [Space]
        _BladeRotationOffset("Blade Rotation Offset", Range(0,3.5)) = 1
        _BladeWidthRange("Blade Width Diversity", Range(0,0.2)) = 0
        _BladeHeightRange("Blade Height Diversity", Range(0,0.5)) = 0
        _BladeWidthMin("Blade Width Minimum", Range(0,0.1)) = 0
        _BladeHeightMin("Blade Height Minimum", Range(0,0.2)) = 0
        _BladeBendCurve("Blade Bend Curve", float) = 1
        _BladeBendDistance("Blade Bend Distance", float) = 0.4
        _BendRotationRandom("Bend Rotation Random", Range(0,1)) = 0.2
        
        [Header(Wind Settings)]
        [Space]
        _WindMap("Wind Map", 2D) = "black" {}
        _WindOffset("Wind Offset", vector) = (0,0,3,0)
        _WindFrequency("Wind Speed",vector) = (0.1,0.1,0.1,0.1)
        _WindStrength("Wind Strength", float) = 0.2

        [Header(Grass Fading Settings)]
        [Space]
        _TessellationFactorMinDistance("Start Fading Distance", float) = 0
        _TessellationFactorMaxDistance("End Fading Distance", float) = 10
        _TessellationFactor("Global Density Factor", Range(0.0, 64.0)) = 4.0
        _TessellationMin("Grass Density Near", Range(1,0)) = 0.2
        _TessellationMax("Grass Density Far", Range(1,0)) = 0.8
        
        [HideInInspector]_BufferCount("BufferCount", int) = 0
        [HideInInspector]_SpecularColor("SpecularColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _DiffusionProfileHash("Diffusion Profile Hash", Float) = 0
        [HideInInspector] _UVMappingMask("_UVMappingMask", Color) = (1, 0, 0, 0)
        [HideInInspector] _UVDetailsMappingMask("_UVDetailsMappingMask", Color) = (1, 0, 0, 0)
        [HideInInspector]_SubsurfaceMask("Subsurface Radius", Range(0.0, 1.0)) = 1.0
        [HideInInspector]_Thickness("Thickness", Range(0.0, 1.0)) = 1.0
        [HideInInspector]_Anisotropy("Anisotropy", Range(-1.0, 1.0)) = 0
        [HideInInspector]_EmissiveExposureWeight("Emissive Pre Exposure", Range(0.0, 1.0)) = 1.0
        [HideInInspector]_AlbedoAffectEmissive("Albedo Affect Emissive", Float) = 0.0
        [HideInInspector] _EmissiveColor("EmissiveColor", Color) = (0, 0, 0)
    }

    HLSLINCLUDE
    #pragma target 4.5
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"

    #define UNITY_PI            3.14159265359f
    #define UNITY_HALF_PI       1.57079632679f
    #define UNITY_TWO_PI        6.28318530718f

    // Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
    // Extended discussion on this function can be found at the following link:
    // https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
    // Returns a number in the 0...1 range.
    float rand(float3 co)
    {
        return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
    }

    // Construct a rotation matrix that rotates around the provided axis, sourced from:
    // https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
    float3x3 AngleAxis3x3(float angle, float3 axis)
    {
        float c, s;
        sincos(angle, s, c);

        float t = 1 - c;
        float x = axis.x;
        float y = axis.y;
        float z = axis.z;

        return float3x3(
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
        );
    }

    struct PlayerPosition
    {
        float3 position;
        float radius;
        float strength;
    };
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "GTGrassLit"
            Tags { "LightMode" = "GTGrassLit" }

            Cull Off

            HLSLPROGRAM

            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

            // #pragma enable_d3d11_debug_symbols

            //enable GPU instancing support
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH
            #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST

            #define HAS_LIGHTLOOP

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _BottomColor, _TopColor, _SpecularColor, _UVMappingMask, _UVDetailsMappingMask;
                half _Power, _AlphaClip, _Smoothness, _Metallic, _DiffusionProfileHash, _SubsurfaceMask, _Thickness, _Anisotropy;
                half4 _BaseColorMap_ST, _DetailMap_ST, _WindMap_ST;
                half _BladeRotationOffset, _BladeHeightRange, _BladeWidthRange, _BladeWidthMin, _BladeHeightMin;
                half _BladeBendCurve, _BladeBendDistance, _BendRotationRandom;
                half2 _WindFrequency;
                half _WindStrength, _TessellationMin, _TessellationMax;
                half3 _WindOffset;
                half _TessellationFactor;
                half _TessellationFactorMinDistance;
                half _TessellationFactorMaxDistance;
                half _LinkDetailsWithBase;
                half _TexWorldScale;
                half _EmissiveExposureWeight;
                half3 _EmissiveColor;
                half _AlbedoAffectEmissive;
                half4 _UVMappingMaskEmissive;
                int _BufferCount;
                StructuredBuffer<PlayerPosition> playerPosBuffer;
            CBUFFER_END
            TEXTURE2D(_BaseColorMap);SAMPLER(sampler_BaseColorMap);
            TEXTURE2D(_WindMap);SAMPLER(sampler_WindMap);
            TEXTURE2D(_HeightMap);SAMPLER(sampler_HeightMap);

            #include "./GTGrassHLSLSupport/GTGrassVaryingMesh.hlsl"
            #include "./GTGrassHLSLSupport/GTGrassVertMesh.hlsl"
            #include "./GTGrassHLSLSupport/GTGrassLitData.hlsl"
            
            PackedVaryingsToDS Vert(AttributesMesh inputMesh)
            {
                VaryingsToDS varyingsType;
                varyingsType.vmesh = VertMesh(inputMesh);
                return PackVaryingsToDS(varyingsType);
            }

            #include "./GTGrassHLSLSupport/GTGrassTessellationShare.hlsl"

            PackedVaryingsMeshToPS GrassBlade(float3 pos, float3 offset, float3x3 transformMatrix, float2 uv, half4 color, float3 normal, float4 tangent)
            {
                VaryingsMeshToPS o;
                ZERO_INITIALIZE(VaryingsMeshToPS, o);

                float3 newPos = TransformWorldToObject(pos) + mul(transformMatrix, offset);
                o.positionRWS.xyz = TransformObjectToWorld(newPos);
                o.positionCS = TransformObjectToHClip(newPos);
                o.texCoord0.xy = uv;
                o.color = color;
                o.normalWS = normal;
                o.tangentWS = tangent;

                return PackVaryingsMeshToPS(o);
            }

            // Geometry Shader
            [maxvertexcount(8)]
            void Geo(triangle PackedVaryingsToDS IN[3], inout TriangleStream<PackedVaryingsMeshToPS> triStream)
            {
                float3 posOS = IN[0].vmesh.interpolators4;
                float3 posWS = IN[0].vmesh.interpolators0;
                float3 vNormal = IN[0].vmesh.interpolators1;
                float4 vTangent = IN[0].vmesh.interpolators2;
                float3 vBitangent = cross(vNormal, vTangent.xyz) * (vTangent.w * unity_WorldTransformParams.w);
                half4 color = IN[0].vmesh.interpolators5;
                float2 uv = IN[0].vmesh.interpolators3;

                // Construct the transform matrix we need
                float3x3 tangentToLocal = float3x3(
                vTangent.x, vBitangent.x, vNormal.x,
                vTangent.y, vBitangent.y, vNormal.y,
                vTangent.z, vBitangent.z, vNormal.z
                );
                float3x3 facingRotationMatrix = AngleAxis3x3(rand(posOS) * UNITY_TWO_PI + _BladeRotationOffset, float3(0, 0, 1));
                float3x3 bendRotationMatrix = AngleAxis3x3((rand(posOS.zzx) * UNITY_PI - UNITY_HALF_PI) * _BendRotationRandom, float3(-1,0,0));
                float3x3 baseTransformationMatrix = mul(tangentToLocal, facingRotationMatrix);
                
                //Wind Movement
                float2 windUV = posWS.xz * _WindMap_ST.xy + _WindMap_ST.zw + _WindFrequency * _Time.y;
                float2 windSample = (SAMPLE_TEXTURE2D_LOD(_WindMap, sampler_WindMap, windUV, 0).xy * 2 - 1) * _WindStrength;
                float3 windAxis = normalize(float3(windSample.x, windSample.y, 0) + _WindOffset);
                //Adjust wind strength depending on the distance to the camera
                float3x3 windMatrix = AngleAxis3x3(UNITY_PI * windSample.x, windAxis);

                float3x3 tipTransformationMatrix = mul(mul(mul(tangentToLocal, windMatrix), bendRotationMatrix), facingRotationMatrix);

                //Grass blade width, height and the forward bending distance
                float width = max(_BladeWidthMin, lerp(uv.x - _BladeWidthRange, uv.x + _BladeWidthRange, rand(posOS.xzy)));
                float height = max(_BladeHeightMin, lerp(uv.y - _BladeHeightRange, uv.y + _BladeHeightRange, rand(posOS.zyx)));
                float forward = rand(posOS.yyz) * _BladeBendDistance;

                //Interaction with Players
                float3 sphereDisp = (float3)0;

                for(int j=0; j < _BufferCount; j++)
                {
                    float3 dir = GetAbsolutePositionWS(posWS) - playerPosBuffer[j].position; // Position comparison
                    float dis = length(dir); // Distance between pos to player position
                    float radius = 1 - saturate(dis / playerPosBuffer[j].radius); // Grass bending strength inside the radius
                    dir *= radius * playerPosBuffer[j].strength; // Multiply strength
                    sphereDisp += dir; // Add up the bending
                }

                sphereDisp = clamp(sphereDisp, -0.8, 0.8);

                //Add vertices with segments in pairs
                for(int i = 0; i < 4; i++)
                {
                    float t = i * 0.333f;
                    float3 offset = float3(width, pow(t, _BladeBendCurve) * forward, height * t);

                    float3x3 transformationMatrix = lerp(tipTransformationMatrix, baseTransformationMatrix, step(i,0.9));

                    // first grass (0) segment does not get displaced by interactivity
                    posWS = posWS + (1-step(i,0.9))*(float3(sphereDisp.x, sphereDisp.y, sphereDisp.z) * t);

                    triStream.Append(GrassBlade(posWS, offset, transformationMatrix, float2(0,t), color, vNormal, vTangent));
                    triStream.Append(GrassBlade(posWS, float3(-offset.x,offset.y,offset.z), transformationMatrix, float2(1,t), color, vNormal, vTangent));
                }

                triStream.RestartStrip();
            }

            // Fragment Shader
            half4 Frag(PackedVaryingsMeshToPS packedInput) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
                FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput);
                
                clip(_TessellationFactorMaxDistance - length(input.positionRWS));
                half4 c = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, input.texCoord0.xy);
                clip(c.a - _AlphaClip);

                // We need to readapt the SS position as our screen space positions are for a low res buffer, but we try to access a full res buffer.
                input.positionSS.xy = _OffScreenRendering > 0 ? (uint2)round(input.positionSS.xy * _OffScreenDownsampleFactor) : input.positionSS.xy;

                uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize();

                // input.positionSS is SV_Position
                PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex);

                float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);

                SurfaceData surfaceData;
                BuiltinData builtinData;
                float3 N = normalize(packedInput.interpolators1);
                GetSurfaceAndBuiltinData(input, V, N, posInput, surfaceData, builtinData);

                BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);

                PreLightData preLightData = GetPreLightData(V, posInput, bsdfData);

                uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_OPAQUE;

                float3 diffuseLighting;
                float3 specularLighting;

                LightLoop(V, posInput, preLightData, bsdfData, builtinData, featureFlags, diffuseLighting, specularLighting);

                diffuseLighting *= GetCurrentExposureMultiplier();
                specularLighting *= GetCurrentExposureMultiplier();

                c = ApplyBlendMode(diffuseLighting, specularLighting, builtinData.opacity);
                c = EvaluateAtmosphericScattering(posInput, V, c);

                c *= lerp(_BottomColor, _TopColor * input.color, saturate(pow(abs(input.texCoord0.y), _Power)));

                return c;
            }

            #pragma vertex Vert
            #pragma require geometry
            #pragma geometry Geo
            #pragma fragment Frag

            #pragma require tessellation tessHW

            #pragma hull Hull
            #pragma domain Domain
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            Cull Off

            ZWrite On
            ZTest LEqual

            ColorMask 0

            HLSLPROGRAM

            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            //enable GPU instancing support
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            // enable dithering LOD crossfade
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            #define SHADERPASS SHADERPASS_SHADOWS

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _BottomColor, _TopColor, _SpecularColor, _UVMappingMask, _UVDetailsMappingMask;
                half _Power, _AlphaClip, _Smoothness, _Metallic, _DiffusionProfileHash, _SubsurfaceMask, _Thickness, _Anisotropy;
                half4 _BaseColorMap_ST, _DetailMap_ST, _WindMap_ST;
                half _BladeRotationOffset, _BladeHeightRange, _BladeWidthRange, _BladeWidthMin, _BladeHeightMin;
                half _BladeBendCurve, _BladeBendDistance, _BendRotationRandom;
                half2 _WindFrequency;
                half _WindStrength, _TessellationMin, _TessellationMax;
                half3 _WindOffset;
                half _TessellationFactor;
                half _TessellationFactorMinDistance;
                half _TessellationFactorMaxDistance;
                half _LinkDetailsWithBase;
                half _TexWorldScale;
                half _EmissiveExposureWeight;
                half3 _EmissiveColor;
                half _AlbedoAffectEmissive;
                half4 _UVMappingMaskEmissive;
                int _BufferCount;
                StructuredBuffer<PlayerPosition> playerPosBuffer;
            CBUFFER_END
            TEXTURE2D(_BaseColorMap);SAMPLER(sampler_BaseColorMap);
            TEXTURE2D(_WindMap);SAMPLER(sampler_WindMap);
            TEXTURE2D(_HeightMap);SAMPLER(sampler_HeightMap);

            #include "./GTGrassHLSLSupport/GTGrassVaryingMesh.hlsl"
            #include "./GTGrassHLSLSupport/GTGrassVertMesh.hlsl"
            #include "./GTGrassHLSLSupport/GTGrassLitData.hlsl"

            PackedVaryingsToDS Vert(AttributesMesh inputMesh)
            {
                VaryingsToDS varyingsType = (VaryingsToDS)0;
                varyingsType.vmesh = VertMesh(inputMesh);
                return PackVaryingsToDS(varyingsType);
            }

            #include "./GTGrassHLSLSupport/GTGrassTessellationShare.hlsl"

            PackedVaryingsMeshToPS GrassBlade(float3 pos, float3 offset, float3x3 transformMatrix, float2 uv)
            {
                VaryingsMeshToPS o;
                ZERO_INITIALIZE(VaryingsMeshToPS, o);

                float3 newPos = TransformWorldToObject(pos) + mul(transformMatrix, offset);
                o.positionRWS.xyz = TransformObjectToWorld(newPos);
                o.positionCS = TransformObjectToHClip(newPos);
                o.texCoord0.xy = uv;

                return PackVaryingsMeshToPS(o);
            }

            // Geometry Shader
            [maxvertexcount(8)]
            void Geo(triangle PackedVaryingsToDS IN[3], inout TriangleStream<PackedVaryingsMeshToPS> triStream)
            {
                float3 posOS = IN[0].vmesh.interpolators4;
                float3 posWS = IN[0].vmesh.interpolators0;
                float3 vNormal = IN[0].vmesh.interpolators1;
                float4 vTangent = IN[0].vmesh.interpolators2;
                float3 vBitangent = cross(vNormal, vTangent.xyz) * (vTangent.w * unity_WorldTransformParams.w);
                half4 color = IN[0].vmesh.interpolators5;
                float2 uv = IN[0].vmesh.interpolators3;

                // Construct the transform matrix we need
                float3x3 tangentToLocal = float3x3(
                vTangent.x, vBitangent.x, vNormal.x,
                vTangent.y, vBitangent.y, vNormal.y,
                vTangent.z, vBitangent.z, vNormal.z
                );
                float3x3 facingRotationMatrix = AngleAxis3x3(rand(posOS) * UNITY_TWO_PI + _BladeRotationOffset, float3(0, 0, 1));
                float3x3 bendRotationMatrix = AngleAxis3x3((rand(posOS.zzx) * UNITY_PI - UNITY_HALF_PI) * _BendRotationRandom, float3(-1,0,0));
                float3x3 baseTransformationMatrix = mul(tangentToLocal, facingRotationMatrix);
                
                //Wind Movement
                float2 windUV = posWS.xz * _WindMap_ST.xy + _WindMap_ST.zw + _WindFrequency * _Time.y;
                float2 windSample = (SAMPLE_TEXTURE2D_LOD(_WindMap, sampler_WindMap, windUV, 0).xy * 2 - 1) * _WindStrength;
                float3 windAxis = normalize(float3(windSample.x, windSample.y, 0) + _WindOffset);
                //Adjust wind strength depending on the distance to the camera
                float3x3 windMatrix = AngleAxis3x3(UNITY_PI * windSample.x, windAxis);

                float3x3 tipTransformationMatrix = mul(mul(mul(tangentToLocal, windMatrix), bendRotationMatrix), facingRotationMatrix);

                //Grass blade width, height and the forward bending distance
                float width = max(_BladeWidthMin, lerp(uv.x - _BladeWidthRange, uv.x + _BladeWidthRange, rand(posOS.xzy)));
                float height = max(_BladeHeightMin, lerp(uv.y - _BladeHeightRange, uv.y + _BladeHeightRange, rand(posOS.zyx)));
                float forward = rand(posOS.yyz) * _BladeBendDistance;

                //Interaction with Players
                float3 sphereDisp = (float3)0;

                for(int j=0; j < _BufferCount; j++)
                {
                    float3 dir = GetAbsolutePositionWS(posWS) - playerPosBuffer[j].position; // Position comparison
                    float dis = length(dir); // Distance between pos to player position
                    float radius = 1 - saturate(dis / playerPosBuffer[j].radius); // Grass bending strength inside the radius
                    dir *= radius * playerPosBuffer[j].strength; // Multiply strength
                    sphereDisp += dir; // Add up the bending
                }

                sphereDisp = clamp(sphereDisp, -0.8, 0.8);

                //Add vertices with segments in pairs
                for(int i = 0; i < 4; i++)
                {
                    float t = i * 0.333f;
                    float3 offset = float3(width, pow(t, _BladeBendCurve) * forward, height * t);

                    float3x3 transformationMatrix = lerp(tipTransformationMatrix, baseTransformationMatrix, step(i,0.9));

                    // first grass (0) segment does not get displaced by interactivity
                    posWS = posWS + (1-step(i,0.9))*(float3(sphereDisp.x, sphereDisp.y, sphereDisp.z) * t);

                    triStream.Append(GrassBlade(posWS, offset, transformationMatrix, float2(0,t)));
                    triStream.Append(GrassBlade(posWS, float3(-offset.x,offset.y,offset.z), transformationMatrix, float2(1,t)));
                }

                triStream.RestartStrip();
            }

            // Fragment Shader
            void Frag(  PackedVaryingsMeshToPS packedInput, out float outputDepth : SV_Depth)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
                FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput);
                
                clip(_TessellationFactorMaxDistance - length(input.positionRWS));
                half4 c = SAMPLE_TEXTURE2D(_BaseColorMap, sampler_BaseColorMap, input.texCoord0.xy);
                clip(c.a - _AlphaClip);

                // input.positionSS is SV_Position
                PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

                float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);

                SurfaceData surfaceData;
                BuiltinData builtinData;
                float3 N = packedInput.interpolators1;
                GetSurfaceAndBuiltinData(input, V, N, posInput, surfaceData, builtinData);

                #if SHADERPASS == SHADERPASS_SHADOWS
                    // If we are using the depth offset and manually outputting depth, the slope-scale depth bias is not properly applied
                    // we need to manually apply.
                    outputDepth = posInput.deviceDepth;
                #endif
            }

            #pragma vertex Vert
            #pragma require geometry
            #pragma geometry Geo

            #pragma require tessellation tessHW

            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Frag

            ENDHLSL
        }
    }
}
