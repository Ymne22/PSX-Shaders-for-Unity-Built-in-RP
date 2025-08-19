Shader "PSX/PSX_Material_Opaque"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _JitterIntensity ("Vertex Jitter", Range(1,500)) = 150
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _JitterIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float  w   : TEXCOORD1;
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

                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = uv * clipPos.w;
                o.w  = clipPos.w;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 affineUV = i.uv / i.w;

                fixed4 col = tex2D(_MainTex, affineUV) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
