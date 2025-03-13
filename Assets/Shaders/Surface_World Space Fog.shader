// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Surface/World Space Fog" {
  Properties
  {
    _Color ("color", Color) = (1,1,1,1)
    _FogColor ("fog color", Color) = (0,0,0,0)
    _FogMaxHeight ("fog start (distance to camera)", float) = 0
    _FogMinHeight ("fog end (distance to camera)", float) = -1
    _UnityFogStartOffset ("unity FogStart offset (distance in units)", float) = 0
    _UnityFogEndOffset ("unity FogEnd offset (distance in units)", float) = 0
  }
  SubShader
  {
    Tags
    { 
      "RenderType" = "Opaque"
    }
    LOD 100
    Pass // ind: 1, name: 
    {
      Tags
      { 
        "LIGHTMODE" = "ForwardBase"
        "RenderType" = "Opaque"
      }
      LOD 100
      ZClip Off
      // m_ProgramMask = 6
      CGPROGRAM
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      
      #define CODE_BLOCK_VERTEX
      //uniform float3 _WorldSpaceCameraPos;
      //uniform float4 _WorldSpaceLightPos0;
      //uniform float4x4 UNITY_MATRIX_MVP;
      //uniform float4x4 UNITY_MATRIX_MV;
      //uniform float4x4 unity_ObjectToWorld;
      //uniform float4x4 unity_WorldToObject;
      //uniform float4 glstate_lightmodel_ambient;
      //uniform float4 unity_FogColor;
      uniform float4 _LightColor0;
      uniform float4 _Color;
      uniform float4 _FogColor;
      uniform float _FogMaxHeight;
      uniform float _FogMinHeight;
      uniform float4 unity_FogStart;
      uniform float4 unity_FogEnd;
      uniform float _UnityFogStartOffset;
      uniform float _UnityFogEndOffset;
      struct appdata_t
      {
          float4 vertex :POSITION;
          float3 normal :NORMAL;
      };
      
      struct OUT_Data_Vert
      {
          float4 xlv_COLOR :COLOR;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float4 xlv_COLOR :COLOR;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target0;
      };
      
      OUT_Data_Vert vert(appdata_t in_v)
      {
          OUT_Data_Vert out_v;
          float4 fog_1;
          float h_2;
          float xlat_varmin_3;
          float xlat_varmax_4;
          float3 diff_5;
          float4 tmpvar_6;
          float4 tmpvar_7;
          tmpvar_7.w = 0;
          tmpvar_7.xyz = float3(in_v.normal);
          float3 tmpvar_8;
          tmpvar_8 = ((_LightColor0.xyz * max(0, dot(normalize(mul(tmpvar_7, unity_WorldToObject).xyz), normalize(_WorldSpaceLightPos0.xyz)))) * _Color.xyz);
          diff_5 = tmpvar_8;
          float3 x_9;
          x_9 = UnityObjectToViewPos(in_v.vertex).xyz;
          float tmpvar_10;
          tmpvar_10 = (_WorldSpaceCameraPos.y + _FogMaxHeight);
          xlat_varmax_4 = tmpvar_10;
          float tmpvar_11;
          tmpvar_11 = (_WorldSpaceCameraPos.y + _FogMinHeight);
          xlat_varmin_3 = tmpvar_11;
          float tmpvar_12;
          tmpvar_12 = clamp(((mul(unity_ObjectToWorld, in_v.vertex).y - xlat_varmin_3) / (xlat_varmax_4 - xlat_varmin_3)), 0, 1);
          h_2 = tmpvar_12;
          float tmpvar_13;
          tmpvar_13 = (unity_FogEnd.x + _UnityFogEndOffset);
          float tmpvar_14;
          tmpvar_14 = (unity_FogStart.x + _UnityFogStartOffset);
          float tmpvar_15;
          float tmpvar_16;
          tmpvar_16 = clamp(((tmpvar_13 - sqrt(dot(x_9, x_9))) / (tmpvar_13 - tmpvar_14)), 0, 1);
          tmpvar_15 = tmpvar_16;
          float4 tmpvar_17;
          tmpvar_17.w = 0;
          tmpvar_17.xyz = float3(diff_5);
          float4 tmpvar_18;
          tmpvar_18 = ((_Color * (glstate_lightmodel_ambient * 2)) + tmpvar_17);
          float4 tmpvar_19;
          tmpvar_19 = lerp(_FogColor, tmpvar_18, float4(h_2, h_2, h_2, h_2));
          fog_1 = tmpvar_19;
          float4 tmpvar_20;
          tmpvar_20 = lerp(unity_FogColor, fog_1, float4(tmpvar_15, tmpvar_15, tmpvar_15, tmpvar_15));
          tmpvar_6 = tmpvar_20;
          out_v.vertex = UnityObjectToClipPos(in_v.vertex);
          out_v.xlv_COLOR = tmpvar_6;
          return out_v;
      }
      
      #define CODE_BLOCK_FRAGMENT
      OUT_Data_Frag frag(v2f in_f)
      {
          OUT_Data_Frag out_f;
          out_f.color = in_f.xlv_COLOR;
          return out_f;
      }
      
      
      ENDCG
      
    } // end phase
  }
  FallBack Off
}
