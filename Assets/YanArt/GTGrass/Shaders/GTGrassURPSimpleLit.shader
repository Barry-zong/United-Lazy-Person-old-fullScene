Shader "YanArt/GTGrassURPSimpleLit"
{
    Properties
    {
        _BaseMap("BaseMap", 2D) = "white" {}
        _AlphaClip("AlphaClip", Range(0,1)) = 0.5
        _BottomColor("BottomColor", Color) = (1,1,1,1)
        [HDR]_TopColor("TopColor", Color) = (1,1,1,1)
        _DiffuseIntensity("Diffuse Intensity", Range(0,2)) = 1
        _AO("Ambient Occlusion", Range(0,1)) = 0.5
        _Power("Color Power", float) = 1

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
        _WindMap("Wind Map", 2D) = "bump" {}
        _WindOffset("Wind Direction Offset", vector) = (0,0,0,0)
        _WindFrequency("Wind Speed",vector) = (0.1,0.1,0.1,0.1)
        _WindStrength("Wind Strength", float) = 0.2

        [Header(Grass Fading Settings)]
        [Space]
        _MinDist("Start Fading Distance", float) = 0
        _MaxDist("End Fading Distance", float) = 10
        _TessellationMin("Grass Density Near", Range(1,0)) = 0.2
        _TessellationMax("Grass Density Far", Range(1,0)) = 0.8

        [Header(Others)]
        [Space]
        _ShadowCalibration("Shadow Bias Offset", vector) = (-1,0,2,0)
        
        [HideInInspector]_BufferCount("BufferCount", int) = 0
    }

    SubShader
    {

        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

        #define UNITY_PI            3.14159265359f
        #define UNITY_HALF_PI       1.57079632679f
        #define UNITY_TWO_PI        6.28318530718f
        ENDHLSL

        Pass
        {
            Name "Grass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex TessellationVertexProgram
            #pragma require geometry
            #pragma geometry geo
            #pragma fragment frag

            #pragma require tessellation tessHW

            #pragma hull hull
            #pragma domain domain

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT

            struct Attributes
            {
                float4 positionOS           : POSITION;
                float2 uv                   : TEXCOORD0;
                float3 normalOS             : NORMAL;
                float4 tangentOS            : TANGENT;
                half3 color                 : COLOR;
            };

            struct ControlPoint
            {
                float4 positionOS           : INTERNALTESSPOS;
                float2 uv                   : TEXCOORD0;
                float3 normalOS             : NORMAL;
                float4 tangentOS            : TANGENT;
                half3 color                 : COLOR;
            };

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside  : SV_InsideTessFactor;
            };

            struct v2g
            {
                float2 uv                   : TEXCOORD0;
                float3 normalOS             : NORMAL;
                float4 tangentOS            : TEXCOORD1;
                float3 positionOS           : TEXCOORD2;
                float3 positionWS           : TEXCOORD3;
                half3 color                 : TEXCOORD4;
            };

            struct g2f
            {
                float4 positionCS           : SV_POSITION;
                float2 uv                   : TEXCOORD0;
                float3 normal               : NORMAL;
                float fogCoord              : TEXCOORD1;
                float3 positionWS           : TEXCOORD2;
                half3 color                 : TEXCOORD3;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord              : TEXCOORD4;
                #endif
            };

            struct PlayerPosition
            {
                float3 position;
                float radius;
                float strength;
            };

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
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BottomColor,_TopColor;
                half _Power, _AO, _DiffuseIntensity, _AlphaClip;
                half4 _BaseMap_ST, _WindMap_ST;
                half _BladeRotationOffset, _BladeHeightRange, _BladeWidthRange, _BladeWidthMin, _BladeHeightMin;
                half _BladeBendCurve, _BladeBendDistance, _BendRotationRandom;
                half2 _WindFrequency;
                half _WindStrength, _TessellationMin, _TessellationMax, _MinDist, _MaxDist;
                half3 _WindOffset;
                half3 _ShadowCalibration;
                int _BufferCount;
                StructuredBuffer<PlayerPosition> playerPosBuffer;
            CBUFFER_END
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            TEXTURE2D(_WindMap);SAMPLER(sampler_WindMap);

            // Vertex Shader
            ControlPoint TessellationVertexProgram(Attributes v)
            {
                ControlPoint p;

                p.positionOS = v.positionOS;
                p.uv = v.uv;
                p.normalOS = v.normalOS;
                p.tangentOS = v.tangentOS;
                p.color = v.color;
                
                return p;
            }

            float tessellationEdgeFactor(ControlPoint vert0, ControlPoint vert1, float distanceFade)
            {
                float3 v0 = vert0.positionOS.xyz;
                float3 v1 = vert1.positionOS.xyz;
                float edgeLength = distance(v0, v1);

                return edgeLength / distanceFade;
            }

            TessellationFactors patchConstantFunc(InputPatch<ControlPoint, 3> patch)
            {
                TessellationFactors f;

                // 计算距离衰减用于tessellation的程度衰减以及风的衰减
                float3 positionWS = TransformObjectToWorld(patch[0].positionOS.xyz);
                float distanceFromCamera = distance(positionWS, _WorldSpaceCameraPos);
                float distanceFade = saturate((distanceFromCamera - _MinDist + 0.00001) / (_MaxDist - _MinDist));
                float distFactor = clamp(distanceFade, _TessellationMin, _TessellationMax);

                f.edge[0] = tessellationEdgeFactor(patch[1], patch[2], distFactor);
                f.edge[1] = tessellationEdgeFactor(patch[2], patch[0], distFactor);
                f.edge[2] = tessellationEdgeFactor(patch[0], patch[1], distFactor);
                f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3;

                return f;
            }

            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [partitioning("integer")]
            [patchconstantfunc("patchConstantFunc")]
            ControlPoint hull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            // This will be used in the Tessellation
            v2g vert(ControlPoint v)
            {
                v2g o = (v2g)0;

                o.positionOS = v.positionOS.xyz;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                o.normalOS = v.normalOS;
                o.tangentOS = v.tangentOS;

                return o;
            }

            [domain("tri")]
            v2g domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                ControlPoint i;

                // Create interpolation macro.
                #define INTERPOLATE(fieldname) i.fieldname = \
                patch[0].fieldname * barycentricCoordinates.x + \
                patch[1].fieldname * barycentricCoordinates.y + \
                patch[2].fieldname * barycentricCoordinates.z;

                INTERPOLATE(positionOS)
                INTERPOLATE(normalOS)
                INTERPOLATE(tangentOS)
                INTERPOLATE(uv)
                INTERPOLATE(color)

                return vert(i);
            }

            g2f GrassBlade(float3 pos, float3 offset, float3x3 transformMatrix, float2 uv, half3 color, half3 normal)
            {
                g2f o = (g2f)0;
                float3 newPos = TransformWorldToObject(pos) + mul(transformMatrix, offset);
                o.positionWS = TransformObjectToWorld(newPos);
                o.positionCS = TransformObjectToHClip(newPos);
                o.fogCoord = ComputeFogFactor(o.positionCS.z);
                o.uv = TRANSFORM_TEX(uv, _BaseMap);
                o.color = color;
                o.normal = TransformObjectToWorldNormal(normal);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                #endif
                return o;
            }

            // Geometry Shader
            [maxvertexcount(8)]
            void geo(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float3 posOS = IN[0].positionOS;
                float3 posWS = IN[0].positionWS;
                float3 vNormal = IN[0].normalOS;
                float4 vTangent = IN[0].tangentOS;
                float3 vBitangent = cross(vNormal, vTangent.xyz) * (vTangent.w * unity_WorldTransformParams.w);
                half3 color = IN[0].color;
                float2 uv = IN[0].uv;

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
                    float3 dir = posWS - playerPosBuffer[j].position; // Position comparison
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

                    triStream.Append(GrassBlade(posWS, offset, transformationMatrix, float2(0,t), color, vNormal));
                    triStream.Append(GrassBlade(posWS, float3(-offset.x,offset.y,offset.z), transformationMatrix, float2(1,t), color, vNormal));
                }

                triStream.RestartStrip();
            }

            // Fragment Shader
            half4 frag(g2f i) : SV_Target
            {
                clip(_MaxDist - distance(i.positionWS,_WorldSpaceCameraPos));
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                clip(c.a - _AlphaClip);

                //Shadow
                float4 shadowCoord;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    shadowCoord = i.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                #else
                    shadowCoord = float4(0, 0, 0, 0);
                #endif

                Light mainLight = GetMainLight(shadowCoord);
                mainLight.shadowAttenuation = lerp(0.0f, 1.0f, saturate(mainLight.shadowAttenuation + _AO));
                half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                half3 diffuseColor = LightingLambert(attenuatedLightColor, mainLight.direction, i.normal);

                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, i.positionWS);
                        light.shadowAttenuation = lerp(0.0f, 1.0f, saturate(light.shadowAttenuation + _AO));
                        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, i.normal);
                    }
                #endif
                
                c.rgb *= diffuseColor;

                c *= lerp(_BottomColor, _TopColor * half4(i.color,1), pow(abs(i.uv.y), _Power)) * _DiffuseIntensity;
                c.rgb = MixFog(c.rgb, i.fogCoord);

                return c;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma require geometry
            #pragma geometry geo

            #pragma require tessellation tessHW

            #pragma hull hull
            #pragma domain domain

            // #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            #ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
                #define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

                // Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
                // For Directional lights, _LightDirection is used when applying shadow Normal Bias.
                // For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
                float3 _LightDirection;
                float3 _LightPosition;

                struct Attributes
                {
                    float4 positionOS           : POSITION;
                    float3 normalOS             : NORMAL;
                    float4 tangentOS            : TANGENT;
                    float2 uv                   : TEXCOORD0;
                };

                struct ControlPoint
                {
                    float4 positionOS           : INTERNALTESSPOS;
                    float2 uv                   : TEXCOORD0;
                    float3 normalOS             : NORMAL;
                    float4 tangentOS            : TANGENT;
                };

                struct TessellationFactors
                {
                    float edge[3] : SV_TessFactor;
                    float inside  : SV_InsideTessFactor;
                };

                struct v2g
                {
                    float2 uv                   : TEXCOORD0;
                    float3 normalOS             : NORMAL;
                    float4 tangentOS            : TEXCOORD1;
                    float3 positionOS           : TEXCOORD2;
                    float3 positionWS           : TEXCOORD3;
                    half3 color                 : TEXCOORD4;
                };

                struct Varyings
                {
                    float2 uv                   : TEXCOORD0;
                    float4 positionCS           : SV_POSITION;
                    float3 positionWS           : TEXCOORD1;
                };

                struct PlayerPosition
                {
                    float3 position;
                    float radius;
                    float strength;
                };

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

                CBUFFER_START(UnityPerMaterial)
                    half4 _BottomColor,_TopColor;
                    half _Power, _AO, _DiffuseIntensity, _AlphaClip;
                    half4 _BaseMap_ST, _WindMap_ST;
                    half _BladeRotationOffset, _BladeHeightRange, _BladeWidthRange, _BladeWidthMin, _BladeHeightMin;
                    half _BladeBendCurve, _BladeBendDistance, _BendRotationRandom;
                    half2 _WindFrequency;
                    half _WindStrength, _TessellationMin, _TessellationMax, _MinDist, _MaxDist;
                    half3 _WindOffset;
                    half3 _ShadowCalibration;
                    int _BufferCount;
                    StructuredBuffer<PlayerPosition> playerPosBuffer;
                CBUFFER_END
                TEXTURE2D(_WindMap);SAMPLER(sampler_WindMap);

                ControlPoint ShadowPassVertex(Attributes v)
                {
                    ControlPoint output;

                    output.uv = v.uv;
                    output.positionOS = v.positionOS;
                    output.normalOS = v.normalOS;
                    output.tangentOS = v.tangentOS;
                    
                    return output;
                }

                float tessellationEdgeFactor(ControlPoint vert0, ControlPoint vert1, float distanceFade)
                {
                    float3 v0 = vert0.positionOS.xyz;
                    float3 v1 = vert1.positionOS.xyz;
                    float edgeLength = distance(v0, v1);

                    return edgeLength / distanceFade;
                }

                TessellationFactors patchConstantFunc(InputPatch<ControlPoint, 3> patch)
                {
                    TessellationFactors f;

                    float3 positionWS = TransformObjectToWorld(patch[0].positionOS.xyz);
                    float distanceFromCamera = distance(positionWS, _WorldSpaceCameraPos);
                    float distanceFade = saturate((distanceFromCamera - _MinDist + 0.00001) / (_MaxDist - _MinDist));
                    float distFactor = clamp(distanceFade, _TessellationMin, _TessellationMax);

                    f.edge[0] = tessellationEdgeFactor(patch[1], patch[2], distFactor);
                    f.edge[1] = tessellationEdgeFactor(patch[2], patch[0], distFactor);
                    f.edge[2] = tessellationEdgeFactor(patch[0], patch[1], distFactor);
                    f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3;

                    return f;
                }

                [domain("tri")]
                [outputcontrolpoints(3)]
                [outputtopology("triangle_cw")]
                [partitioning("integer")]
                [patchconstantfunc("patchConstantFunc")]
                ControlPoint hull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
                {
                    return patch[id];
                }

                // This will be used in the Tessellation
                v2g vert(ControlPoint v)
                {
                    v2g o = (v2g)0;

                    o.positionOS = v.positionOS.xyz;
                    o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                    o.uv = v.uv;
                    o.normalOS = v.normalOS;
                    o.tangentOS = v.tangentOS;

                    return o;
                }

                [domain("tri")]
                v2g domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
                {
                    ControlPoint i;

                    // Create interpolation macro.
                    #define INTERPOLATE(fieldname) i.fieldname = \
                    patch[0].fieldname * barycentricCoordinates.x + \
                    patch[1].fieldname * barycentricCoordinates.y + \
                    patch[2].fieldname * barycentricCoordinates.z;

                    INTERPOLATE(positionOS)
                    INTERPOLATE(normalOS)
                    INTERPOLATE(tangentOS)
                    INTERPOLATE(uv)

                    return vert(i);
                }

                float4 GetShadowPositionHClip(float3 positionOS, float3 normalOS)
                {
                    float3 positionWS = TransformObjectToWorld(positionOS);
                    float3 normalWS = TransformObjectToWorldNormal(normalOS);
                    normalWS += _ShadowCalibration;

                    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                    #else
                        float3 lightDirectionWS = _LightDirection;
                    #endif

                    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                    #if UNITY_REVERSED_Z
                        positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                    #else
                        positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                    #endif

                    return positionCS;
                }

                Varyings GrassBlade(float3 pos, float3 offset, float3x3 transformMatrix, float2 uv, float3 normalOS)
                {
                    Varyings o = (Varyings)0;
                    float3 newPos = TransformWorldToObject(pos) + mul(transformMatrix, offset);
                    o.positionWS = TransformObjectToWorld(newPos);
                    o.positionCS = GetShadowPositionHClip(newPos, normalOS);
                    o.uv = TRANSFORM_TEX(uv, _BaseMap);
                    return o;
                }

                // Geometry Shader
                [maxvertexcount(8)]
                void geo(triangle v2g IN[3], inout TriangleStream<Varyings> triStream)
                {
                    float3 posOS = IN[0].positionOS;
                    float3 posWS = IN[0].positionWS;
                    float3 vNormal = IN[0].normalOS;
                    float4 vTangent = IN[0].tangentOS;
                    float3 vBitangent = cross(vNormal, vTangent.xyz) * (vTangent.w * unity_WorldTransformParams.w);
                    // float3 newNormalTBN = vBitangent;

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
                    float width = max(_BladeWidthMin, lerp(IN[0].uv.x - _BladeWidthRange, IN[0].uv.x + _BladeWidthRange, rand(posOS.xzy)));
                    float height = max(_BladeHeightMin, lerp(IN[0].uv.y - _BladeHeightRange, IN[0].uv.y + _BladeHeightRange, rand(posOS.zyx)));
                    float forward = rand(posOS.yyz) * _BladeBendDistance;

                    //Interaction with Players
                    float3 sphereDisp = (float3)0;

                    for(int j=0; j < _BufferCount; j++)
                    {
                        float3 dir = posWS - playerPosBuffer[j].position; // Position comparison
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

                        triStream.Append(GrassBlade(posWS, offset, transformationMatrix, float2(0,t), vNormal));
                        triStream.Append(GrassBlade(posWS, float3(-offset.x,offset.y,offset.z), transformationMatrix, float2(1,t), vNormal));
                    }

                    triStream.RestartStrip();
                }

                half4 ShadowPassFragment(Varyings input) : SV_TARGET
                {
                    half alpha = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a;
                    clip(alpha - _AlphaClip);
                    Alpha(alpha, half4(1,1,1,1), 0);
                    return 0;
                }

            #endif
            ENDHLSL
        }
    }
}
