// Upgrade NOTE: commented out 'float4 unity_DynamicLightmapST', a built-in variable
// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: commented out 'float4 unity_DynamicLightmapST', a built-in variable
// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/Glow"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _RimColor ("Rim Color", Color) = (1,1,1,1)
    _RimPower ("Rim Power", Range(0.5, 6)) = 3
  }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _RimColor;
        half _RimPower;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // 計算視線方向向量（標準化）
            float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
            // 計算視線方向向量和表面法向量的內積，得到夾角
            half rim = dot(viewDir, o.Normal);
            // 反轉內積值，使邊緣光暈在面向相機時更加明亮
            rim = 1 - saturate(rim);
            // 將rim值提升到_RimPower次方以控制光暈的強度
            rim = pow(rim, _RimPower);

            // 將邊緣光暈顏色與指定亮度添加到最終顏色中
            o.Emission = _RimColor.rgb * rim;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
