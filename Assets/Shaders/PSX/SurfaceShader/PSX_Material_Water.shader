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

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert alpha:fade fullforwardshadows

        fixed4 _WaterColor;
        sampler2D _MainTex;
        float _TextureScrollSpeedX;
        float _TextureScrollSpeedY;
        sampler2D _SecondTex;
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

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            o.uv_MainTex = v.texcoord;

            float4x4 modelView = mul(UNITY_MATRIX_V, unity_ObjectToWorld);
            float4 viewPos = mul(modelView, v.vertex);
            viewPos.xyz = floor(viewPos.xyz * _JitterIntensity) / _JitterIntensity;
            
            float4x4 inverseModelView = unity_WorldToObject;
            inverseModelView = mul(inverseModelView, UNITY_MATRIX_I_V);
            v.vertex = mul(inverseModelView, viewPos);

            if (_EnableWaves > 0.5)
            {
                float waveOffset = sin(_Time.y * _WaveSpeed + v.vertex.x * _WaveFrequency + v.vertex.z * _WaveFrequency) * _WaveAmplitude;
                v.vertex.y += waveOffset;
            }

            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.viewDir = WorldSpaceViewDir(v.vertex);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Normal = float3(0,1,0); 

            float2 animatedUVsDistortion = IN.uv_MainTex + float2(_Time.y * _SecondTextureScrollSpeedX, _Time.y * _SecondTextureScrollSpeedY);
            fixed4 distortionTexColor = tex2D(_SecondTex, animatedUVsDistortion);

            float2 animatedUVsPrimary = IN.uv_MainTex + float2(_Time.y * _TextureScrollSpeedX, _Time.y * _TextureScrollSpeedY);

            float2 distortionOffset = (distortionTexColor.rg * 2.0 - 1.0) * _DistortionStrength;
            
            float distToRippleCenter = distance(IN.worldPos.xz, _RippleCenterStatic.xz);
            float rippleWave = sin((distToRippleCenter * _RippleDensityStatic) - (_Time.y * _RippleSpeedStatic)) * _RippleStrengthStatic;
            distortionOffset += float2(rippleWave, rippleWave);

            animatedUVsPrimary += distortionOffset;

            fixed4 primaryTexColor = tex2D(_MainTex, animatedUVsPrimary);

            fixed4 finalColor = _WaterColor;
            finalColor.rgb *= primaryTexColor.rgb;
            finalColor.a *= primaryTexColor.a;
            finalColor.rgb = floor(finalColor.rgb * _ColorQuantizationLevels) / _ColorQuantizationLevels;

            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}