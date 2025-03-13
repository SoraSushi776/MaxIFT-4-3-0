// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Default Fog Color"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _FogMultipler ("Fog Multipler", Range(0, 1)) = 1
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader
  {
    Tags
    { 
      "IGNOREPROJECTOR" = "true"
      "QUEUE" = "Transparent"
      "RenderType" = "TransparentCutout"
    }
    Pass // ind: 1, name: 
    {
      Tags
      { 
        "IGNOREPROJECTOR" = "true"
        "QUEUE" = "Transparent"
        "RenderType" = "TransparentCutout"
      }
      ZClip Off
      ZWrite Off
      Cull Off
      Blend SrcAlpha OneMinusSrcAlpha
      // m_ProgramMask = 6
      CGPROGRAM
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      
      #define CODE_BLOCK_VERTEX
      #define UNITY_FOG_COORDS(idx) float4 fogCoord : TEXCOORD##idx;
      #pragma multi_compile_fog
      //uniform float4x4 UNITY_MATRIX_MVP;
      uniform float4 _MainTex_ST;
      uniform sampler2D _MainTex;
      uniform float4 _Color;
      struct appdata_t
      {
          float4 vertex :POSITION;
          float4 color :COLOR;
          float4 texcoord :TEXCOORD0;
          UNITY_VERTEX_INPUT_INSTANCE_ID
      };
      
      struct OUT_Data_Vert
      {
          float4 xlv_COLOR :COLOR;
          float2 xlv_TEXCOORD0 :TEXCOORD0;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float4 xlv_COLOR :COLOR;
          float2 xlv_TEXCOORD0 :TEXCOORD0;
          UNITY_FOG_COORDS(1)
          float4 vertex :SV_POSITION;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target0;
      };

v2f vert(appdata_t in_v)
{
    v2f out_v;
    out_v.vertex = UnityObjectToClipPos(in_v.vertex);
    out_v.xlv_COLOR = in_v.color;
    out_v.xlv_TEXCOORD0 = TRANSFORM_TEX(in_v.texcoord.xy, _MainTex);
    UNITY_TRANSFER_FOG(out_v, out_v.vertex);
    return out_v;
}

OUT_Data_Frag frag(v2f in_f)
{
    OUT_Data_Frag out_f;
    float4 col_1;
    float4 tex_2;
    float4 tmpvar_3;
    tmpvar_3 = tex2D(_MainTex, in_f.xlv_TEXCOORD0);
    tex_2 = tmpvar_3;
    float3 tmpvar_4;
    tmpvar_4 = _Color.xyz;
    col_1.xyz = float3(tmpvar_4);
    col_1.w = ((in_f.xlv_COLOR.w * tex_2.w) * _Color.w);
    float4 tmpvar_5;
    tmpvar_5 = col_1;
    col_1 = tmpvar_5;
    out_f.color = col_1;
    UNITY_APPLY_FOG(in_f.fogCoord.x, out_f.color);
    return out_f;
}
      
      
      ENDCG
      
    } // end phase
  }
  FallBack Off
}