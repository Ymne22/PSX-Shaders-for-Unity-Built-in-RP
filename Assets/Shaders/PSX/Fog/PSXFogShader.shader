Shader "Hidden/PSXFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float3 viewRay : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _FogColor;
            float _FogStartDistance;
            float _FogEndDistance;
            float _CoverSky;
            float4x4 _InverseProjection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float4 clip = float4(v.uv * 2.0 - 1.0, 1.0, 1.0);
                float4 viewRay = mul(_InverseProjection, clip);
                o.viewRay = viewRay.xyz / viewRay.w;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float linearDepth = LinearEyeDepth(depth);
                float fogAmount = saturate((linearDepth - _FogStartDistance) / (_FogEndDistance - _FogStartDistance));
                if (_CoverSky > 0.5 && depth >= 0.9999)
                    fogAmount = 1.0;
                return lerp(col, _FogColor, fogAmount);
            }
            ENDCG
        }
    }
}