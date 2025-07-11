Shader "PSX/PSX_Material_DitherTransparent"
{
    Properties
    {
        _Color ("Albedo Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _JitterIntensity ("Jitter Intensity", Range(10, 500)) = 150
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
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _JitterIntensity;
        float _Transparency;
        float _DitherScale;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        static const float4x4 ditherMatrix = {
            0,  8,  2,  10,
            12, 4,  14, 6,
            3,  11, 1,  9,
            15, 7,  13, 5
        };

        float GetDitherValue(float2 screenPos)
        {
            screenPos *= _ScreenParams.xy * _DitherScale;
            
            uint x = (uint)screenPos.x % 4;
            uint y = (uint)screenPos.y % 4;
            
            return ditherMatrix[x][y] / 16.0;
        }

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            float4x4 modelView = mul(UNITY_MATRIX_V, unity_ObjectToWorld);
            float4 viewPos = mul(modelView, v.vertex);
            viewPos.xyz = floor(viewPos.xyz * _JitterIntensity) / _JitterIntensity;
            
            float4x4 inverseModelView = unity_WorldToObject;
            inverseModelView = mul(inverseModelView, UNITY_MATRIX_I_V);
            
            v.vertex = mul(inverseModelView, viewPos);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb *= _Color.rgb;
            o.Albedo = c.rgb;
            
            float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
            float dither = GetDitherValue(screenPos);
            
            o.Alpha = (c.a * _Transparency > dither) ? 1 : 0;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}