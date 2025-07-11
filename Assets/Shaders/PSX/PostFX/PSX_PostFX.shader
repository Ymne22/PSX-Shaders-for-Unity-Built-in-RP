Shader "Hidden/PSX_PostFX"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorDepth ("Color Depth", Range(2, 256)) = 32
        _DitherIntensity ("Dither Intensity", Range(0, 1)) = 1.0
        _PixelSize ("Pixel Size", Vector) = (320, 240, 0, 0)
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
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
            };

            sampler2D _MainTex;
            float _ColorDepth;
            float _DitherIntensity;
            float2 _PixelSize;
            
            float bayerDither(float2 position)
            {
                const float2x2 bayerMatrix = { 0, 2, 3, 1 };
                int2 p = int2(fmod(position.x, 2), fmod(position.y, 2));
                return bayerMatrix[p.y][p.x] / 4.0;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixelated_uv = floor(i.uv * _PixelSize) / _PixelSize;
                fixed4 col = tex2D(_MainTex, pixelated_uv);

                float2 ditherPos = i.uv * _PixelSize;
                float dither = (bayerDither(ditherPos) - 0.5) * _DitherIntensity;
                col.rgb += dither / (_ColorDepth - 1.0);

                col.rgb = floor(col.rgb * _ColorDepth) / (_ColorDepth - 1.0);
                
                return col;
            }
            ENDCG
        }
    }
}