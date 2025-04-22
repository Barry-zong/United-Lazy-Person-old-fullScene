Shader "YanArt/GTGrassUnlit"
{
    Properties
    {
        _BaseMap("BaseMap", 2D) = "white" {}
        _AlphaClip("AlphaClip", Range(0,0.5)) = 0.5
        _BottomColor("BottomColor", Color) = (1,1,1,1)
        [HDR]_TopColor("TopColor", Color) = (1,1,1,1)
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
        _WindOffset("Wind Offset", vector) = (0,0,0,0)
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
        _ShadowCalibration("Shadow Offset", vector) = (-1,0,2,0)
        
        [HideInInspector]_BufferCount("BufferCount", int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        Cull Off

        Pass
        {
            Name "Grass"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
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

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float3 normalOS         : NORMAL;
                float4 tangentOS        : TANGENT;
                fixed3 color            : COLOR;
            };

            struct ControlPoint
            {
                float4 positionOS       : INTERNALTESSPOS;
                float2 uv               : TEXCOORD0;
                float3 normalOS         : NORMAL;
                float4 tangentOS        : TANGENT;
                fixed3 color            : COLOR;
            };

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside  : SV_InsideTessFactor;
            };

            struct v2g
            {
                float2 uv                   : TEXCOORD0;
                float3 normalOS             : TEXCOORD1;
                float4 tangentOS            : TEXCOORD2;
                float3 positionOS           : TEXCOORD3;
                float3 positionWS           : TEXCOORD4;
                fixed3 color                : TEXCOORD5;
            };

            struct g2f
            {
                float4 pos                  : SV_POSITION;
                float2 uv                   : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float3 positionWS           : TEXCOORD2;
                fixed3 color                : TEXCOORD3;
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
            
            fixed4 _BottomColor,_TopColor;
            fixed _Power, _AlphaClip;
            fixed _BladeRotationOffset, _BladeHeightRange, _BladeWidthRange, _BladeWidthMin, _BladeHeightMin;
            fixed _BladeBendCurve, _BladeBendDistance, _BendRotationRandom;
            fixed2 _WindFrequency;
            fixed _WindStrength, _TessellationMin, _TessellationMax, _MinDist, _MaxDist;
            fixed3 _WindOffset;
            fixed3 _ShadowCalibration;
            int _BufferCount;
            StructuredBuffer<PlayerPosition> playerPosBuffer;

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            sampler2D _WindMap;
            float4 _WindMap_ST;

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

                float3 positionWS = mul(unity_ObjectToWorld,patch[0].positionOS).xyz;
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
                o.positionWS = mul(unity_ObjectToWorld,v.positionOS).xyz;
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

            g2f GrassBlade(float3 pos, float3 offset, float3x3 transformMatrix, float2 uv, fixed3 color)
            {
                g2f o = (g2f)0;
                float3 newPos = mul(unity_WorldToObject,float4(pos,1)).xyz + mul(transformMatrix, offset);
                o.positionWS = mul(unity_ObjectToWorld,float4(newPos,1)).xyz;
                o.pos = UnityObjectToClipPos(newPos);
                UNITY_TRANSFER_FOG(o,o.pos);
                o.uv = TRANSFORM_TEX(uv, _BaseMap);
                o.color = color;
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
                fixed3 color = IN[0].color;
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
                float4 uvMipmap = float4(windUV,0,0); 
                float2 windSample = (tex2Dlod(_WindMap, uvMipmap).xy * 2 - 1) * _WindStrength;
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
                    posWS = posWS + (1-step(i,0.9))*(float3(sphereDisp.x, sphereDisp.y, sphereDisp.z) * t);

                    triStream.Append(GrassBlade(posWS, offset, transformationMatrix, float2(0,t), color));
                    triStream.Append(GrassBlade(posWS, float3(-offset.x,offset.y,offset.z), transformationMatrix, float2(1,t), color));
                }

                triStream.RestartStrip();
            }

            // Fragment Shader
            fixed4 frag(g2f i) : SV_Target
            {
                clip(_MaxDist - distance(i.positionWS,_WorldSpaceCameraPos));
                fixed4 c = tex2D(_BaseMap, i.uv);
                clip(c.a - _AlphaClip);
                
                c *= lerp(_BottomColor, _TopColor * half4(i.color,1), pow(abs(i.uv.y), _Power));
                UNITY_APPLY_FOG(i.fogCoord, c);

                return c;
            }
            ENDCG
        }
    }
}
