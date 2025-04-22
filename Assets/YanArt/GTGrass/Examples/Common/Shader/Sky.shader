Shader "YanArt/Common/Sky"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GroundColor("GroundColor", Color) = (1,1,1,1)
        _SkyColor("SkyColor", Color) = (1, 1, 1, 1)
        _Pow("Sky Power", float) = 1
        _Offset("Offset", float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 positionOS : TEXCOORD1;
            };
            
            fixed4 _SkyColor, _GroundColor;
            fixed _Pow, _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.positionOS = v.vertex.xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = lerp(_GroundColor, _SkyColor, saturate(pow(max(0,i.positionOS.y + _Offset), _Pow)));
                return col;
            }
            ENDCG
        }
    }
}
