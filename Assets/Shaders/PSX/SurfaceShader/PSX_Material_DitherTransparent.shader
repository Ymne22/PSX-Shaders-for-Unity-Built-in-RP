Shader "PSX/PSX_DitherTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _JitterIntensity ("Vertex Jitter", Range(10, 500)) = 150
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _DitherScale ("Dither Scale", Float) = 1.0
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _JitterIntensity;
            float _Transparency;
            float _DitherScale;

            static const float4x4 ditherMatrix = {
                0,  8,  2, 10,
                12, 4, 14,  6,
                3, 11,  1,  9,
                15, 7, 13,  5
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float  w        : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;

                float4x4 modelView = mul(UNITY_MATRIX_V, unity_ObjectToWorld);
                float4 viewPos = mul(modelView, v.vertex);
                viewPos.xyz = floor(viewPos.xyz * _JitterIntensity) / _JitterIntensity;

                float4x4 inverseModelView = mul(unity_WorldToObject, UNITY_MATRIX_I_V);
                float4 worldPos = mul(inverseModelView, viewPos);

                float4 clipPos = UnityObjectToClipPos(worldPos);
                o.pos = clipPos;

                // affine trick
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = uv * clipPos.w;
                o.w  = clipPos.w;

                o.screenPos = ComputeScreenPos(clipPos);
                return o;
            }

            float GetDitherValue(float2 screenPos)
            {
                screenPos *= _ScreenParams.xy * _DitherScale;

                uint x = (uint)screenPos.x % 4;
                uint y = (uint)screenPos.y % 4;

                return ditherMatrix[y][x] / 16.0;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 affineUV = i.uv / i.w;
                fixed4 col = tex2D(_MainTex, affineUV) * _Color;

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float dither = GetDitherValue(screenUV);
                float alpha = (col.a * _Transparency > dither) ? 1.0 : 0.0;

                col.a = alpha;
                return col;
            }
            ENDCG
        }
    }
}
