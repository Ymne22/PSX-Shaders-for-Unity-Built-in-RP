Shader "PSX/PSX_Material_Opaque"
{
    Properties
    {
        _Color ("Albedo Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _JitterIntensity ("Jitter Intensity", Range(10, 500)) = 150
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        float _JitterIntensity;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert (inout appdata_full v)
        {
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
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}