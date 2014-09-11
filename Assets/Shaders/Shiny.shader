Shader "Custom/Shiny" 
{	
	Properties // http://docs.unity3d.com/Manual/SL-Properties.html http://docs.unity3d.com/Manual/SL-PropertiesInPrograms.html
	{
		_SpecularRadianceTex ("SpecularRadianceTex", CUBE) = "" {}
	}

	SubShader // http://docs.unity3d.com/Manual/SL-SubShader.html
	{
		Pass // sets state as well - http://docs.unity3d.com/Manual/SL-Pass.html
		{
			Tags // http://docs.unity3d.com/Manual/SL-PassTags.html
			{
				"LightMode" = "Vertex" // "ForwardBase" 
				"RenderType" = "Opaque"
			}
			CGPROGRAM	
			
			// http://docs.unity3d.com/Manual/SL-ShaderPrograms.html
			#pragma target 3.0
			#pragma glsl // otherwise target 3.0 on OpenGL/OSX has 1024 instructions limit! 4.0/5.0 are DX11 only
			#pragma vertex vs
			#pragma fragment ps
			
			#include "UnityCG.cginc" // http://docs.unity3d.com/Manual/SL-BuiltinIncludes.html
			
			// --- Properties
			samplerCUBE _SpecularRadianceTex;
			struct vertexOut 
			{
				float4 pos : SV_POSITION;
				float3 wnormal : TEXCOORD0;
			};
			
			vertexOut vs (appdata_base v) // http://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
			{
				vertexOut o;
				//float3 objectSpaceLPos = mul(unity_LightPosition[0],UNITY_MATRIX_IT_MV); 
				//float lightDistance = distance(v.vertex.xyz, objectSpaceLPos);
				
				// NOTE --- input geometry is assumed to be a unit sphere at zero
				float3 worldNormal = normalize(mul(UNITY_MATRIX_IT_MV, float4(v.normal,0)).xyz);
				
				o.pos = mul (UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
				o.wnormal = worldNormal;
				
				return o;
			}
			
			half4 ps (vertexOut i) : COLOR
			{
				float3 col = texCUBE(_SpecularRadianceTex, float4(i.wnormal, 0) ).rgb;
				
				// Reinhard
				col *= 4;
				col = col / (1.0.xxx + col);
				col = pow(col,1/2.2);
				
				return half4 (col, 1);
			}
			
			ENDCG
		}
	}
	
	Fallback off // http://docs.unity3d.com/Manual/SL-Fallback.html
}
