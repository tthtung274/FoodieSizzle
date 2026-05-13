Shader "MoonflowerCarnivore/Dissolve Edge" {
	Properties {
		[Enum(Off,0,Front,1,Back,2)] _CullMode ("Culling Mode", Float) = 0
		[Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 0
		_Progress ("Progress", Range(0, 1)) = 0
		_MainTex ("Main Texture", 2D) = "white" {}
		_DissolveTex ("Dissolve Texture", 2D) = "white" {}
		_Edge ("Edge", Range(0.01, 0.5)) = 0.01
		[Header(Edge Color)] [Toggle(EDGE_COLOR)] _UseEdgeColor ("Edge Color?", Float) = 1
		[HideIfDisabled(EDGE_COLOR)] [NoScaleOffset] _EdgeAroundRamp ("Edge Ramp", 2D) = "white" {}
		[HideIfDisabled(EDGE_COLOR)] _EdgeAround ("Edge Color Range", Range(0, 0.5)) = 0
		[HideIfDisabled(EDGE_COLOR)] _EdgeAroundPower ("Edge Color Power", Range(1, 5)) = 1
		[HideIfDisabled(EDGE_COLOR)] _EdgeAroundHDR ("Edge Color HDR", Range(0, 10)) = 1
		[HideIfDisabled(EDGE_COLOR)] _EdgeDistortion ("Edge Distortion", Range(0, 1)) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
}