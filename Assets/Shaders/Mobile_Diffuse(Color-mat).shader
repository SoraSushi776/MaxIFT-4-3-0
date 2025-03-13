Shader "Mobile/Diffuse(Color-mat)" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
	}
	//DummyShaderTextExporter
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 250

CGPROGRAM
#pragma surface surf Lambert noforwardadd

		fixed4 _Color;

struct Input {
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
    o.Albedo = _Color.rgb;
    o.Alpha = _Color.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}