Shader "Custom/CustomShaderTest" 
{
	// - rendering pipeline: http://docs.unity3d.com/Manual/SL-RenderPipeline.html
	// -- http://docs.unity3d.com/Manual/RenderTech-ForwardRendering.html
	
	// - simple example: http://docs.unity3d.com/Manual/ShaderTut2.html http://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
	// - platfomr #defines (auto-included HLSLSupport.cginc): http://docs.unity3d.com/Manual/SL-BuiltinMacros.html
	// - built-in uniforms: http://docs.unity3d.com/Manual/SL-BuiltinStateInPrograms.html
	// - more uniforms: http://docs.unity3d.com/Manual/SL-BuiltinValues.html
	
	Properties // http://docs.unity3d.com/Manual/SL-Properties.html http://docs.unity3d.com/Manual/SL-PropertiesInPrograms.html
	{
		//_Color ("Main Color", Color) = (1,1,1,0.5)
		//_MainTex ("Texture", 2D) = "white" { }
	}

	SubShader // http://docs.unity3d.com/Manual/SL-SubShader.html
	{
		Pass // sets state as well - http://docs.unity3d.com/Manual/SL-Pass.html
		{
			Tags // http://docs.unity3d.com/Manual/SL-PassTags.html
			{
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM	
			
			#pragma vertex vs
			#pragma fragment ps
			
			#include "UnityCG.cginc" // http://docs.unity3d.com/Manual/SL-BuiltinIncludes.html
			
			// --- Properties
			//float4 _Color;
			//sampler2D _MainTex;
			//float4 _MainTex_ST; // for TRANSFORM_TEX

			// --- Shader code
			struct vertexOut 
			{
				float4 pos : SV_POSITION;
				float3 color : COLOR0;
			};
			
			vertexOut vs (appdata_base v) // http://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
			{
				vertexOut o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex 
					+ float4(v.normal * sin(_Time * 5) * frac(v.vertex.x * 100) * 0.25, 0) );
				o.color = v.normal * 0.5 + 0.5;
				
				return o;
			}
			
			half4 ps (vertexOut i) : COLOR
			{
				return half4 (i.color, 1);
			}
			
			ENDCG
		}
	}
	
	Fallback off // http://docs.unity3d.com/Manual/SL-Fallback.html
}
