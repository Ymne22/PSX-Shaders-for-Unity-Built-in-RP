Shader "PSX/PSX_Material_Water"
{
    Properties
    {
        _WaterColor ("Water Base Color", Color) = (0.2,0.4,0.6,0.8)
        _MainTex ("Primary Albedo Texture (RGBA)", 2D) = "white" {}
        _TextureScrollSpeedX ("Primary Tex Scroll Speed X", Range(-2.0, 2.0)) = 0.05
        _TextureScrollSpeedY ("Primary Tex Scroll Speed Y", Range(-2.0, 2.0)) = 0.05
        _SecondTex ("Distortion Texture (RGBA)", 2D) = "gray" {}
        _SecondTextureScrollSpeedX ("Distortion Tex Scroll Speed X", Range(-2.0, 2.0)) = 0.03
        _SecondTextureScrollSpeedY ("Distortion Tex Scroll Speed Y", Range(-2.0, 2.0)) = 0.03
        _DistortionStrength ("Texture Distortion Strength", Range(0.0, 0.5)) = 0.05
        _JitterIntensity ("Vertex Jitter Intensity", Range(10, 500)) = 150
        _EnableWaves ("Enable Geometric Waves", Float) = 1.0
        _WaveAmplitude ("Wave Amplitude", Range(0.01, 1.0)) = 0.1
        _WaveFrequency ("Wave Frequency", Range(0.1, 10.0)) = 2.0
        _WaveSpeed ("Wave Speed", Range(0.1, 5.0)) = 1.0
        _ColorQuantizationLevels ("Color Quantization Levels", Range(1, 64)) = 16
        _RippleCenterStatic ("Procedural Ripple Center (World XZ)", Vector) = (0,0,0,0)
        _RippleSpeedStatic ("Procedural Ripple Speed", Range(0.1, 5.0)) = 1.0
        _RippleStrengthStatic ("Procedural Ripple Strength", Range(0.0, 0.2)) = 0.05
        _RippleDensityStatic ("Procedural Ripple Density", Range(1.0, 20.0)) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SecondTex;
            float4 _SecondTex_ST;

            float4 _WaterColor;
            float _TextureScrollSpeedX;
            float _TextureScrollSpeedY;
            float _SecondTextureScrollSpeedX;
            float _SecondTextureScrollSpeedY;
            float _DistortionStrength;
            float _JitterIntensity;
            float _EnableWaves;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _ColorQuantizationLevels;
            float4 _RippleCenterStatic;
            float _RippleSpeedStatic;
            float _RippleStrengthStatic;
            float _RippleDensityStatic;

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
                float3 worldPos : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;

                float4x4 modelView = mul(UNITY_MATRIX_V, unity_ObjectToWorld);
                float4 viewPos = mul(modelView, v.vertex);
                viewPos.xyz = floor(viewPos.xyz * _JitterIntensity) / _JitterIntensity;

                float4x4 inverseModelView = mul(unity_WorldToObject, UNITY_MATRIX_I_V);
                float4 worldPos = mul(inverseModelView, viewPos);

                if (_EnableWaves > 0.5)
                {
                    float waveOffset = sin(_Time.y * _WaveSpeed + worldPos.x * _WaveFrequency + worldPos.z * _WaveFrequency) * _WaveAmplitude;
                    worldPos.y += waveOffset;
                }

                float4 clipPos = UnityObjectToClipPos(worldPos);
                o.pos = clipPos;

                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = uv * clipPos.w;
                o.w  = clipPos.w;

                o.worldPos = mul(unity_ObjectToWorld, worldPos).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv / i.w;

                float2 uvDistort = uv + float2(_Time.y * _SecondTextureScrollSpeedX, _Time.y * _SecondTextureScrollSpeedY);
                fixed4 distortionTex = tex2D(_SecondTex, uvDistort);

                float2 uvPrimary = uv + float2(_Time.y * _TextureScrollSpeedX, _Time.y * _TextureScrollSpeedY);

                float2 distortionOffset = (distortionTex.rg * 2.0 - 1.0) * _DistortionStrength;

                float distToRippleCenter = distance(i.worldPos.xz, _RippleCenterStatic.xz);
                float rippleWave = sin((distToRippleCenter * _RippleDensityStatic) - (_Time.y * _RippleSpeedStatic)) * _RippleStrengthStatic;
                distortionOffset += float2(rippleWave, rippleWave);

                uvPrimary += distortionOffset;

                fixed4 texCol = tex2D(_MainTex, uvPrimary);
                fixed4 finalCol = _WaterColor;
                finalCol.rgb *= texCol.rgb;
                finalCol.a   *= texCol.a;
                finalCol.rgb = floor(finalCol.rgb * _ColorQuantizationLevels) / _ColorQuantizationLevels;

                return finalCol;
            }
            ENDCG
        }
    }
}
