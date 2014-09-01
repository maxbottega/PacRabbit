Shader "Custom/CustomShaderTest" 
{
	// - rendering pipeline: http://docs.unity3d.com/Manual/SL-RenderPipeline.html
	// -- http://docs.unity3d.com/Manual/RenderTech-ForwardRendering.html
	
	// - simple examples: http://docs.unity3d.com/Manual/ShaderTut2.html http://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
	// - platform #defines (auto-included HLSLSupport.cginc): http://docs.unity3d.com/Manual/SL-BuiltinMacros.html
	// - built-in uniforms (some are outdated): http://docs.unity3d.com/Manual/SL-BuiltinValues.html (http://docs.unity3d.com/Manual/SL-BuiltinStateInPrograms.html)
	// - http://en.wikibooks.org/wiki/Cg_Programming/Unity http://en.wikibooks.org/wiki/Cg_Programming/Unity/Multiple_Lights
	
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
				"LightMode" = "Vertex" // "ForwardBase" 
				"RenderType" = "Opaque"
			}
			CGPROGRAM	
			
			// http://docs.unity3d.com/Manual/SL-ShaderPrograms.html
			#pragma target 3.0
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
				
				float3 objectSpaceLPos = mul(unity_LightPosition[0],UNITY_MATRIX_IT_MV); 
				float lightDistance = distance(v.vertex.xyz, objectSpaceLPos);
				float lightFalloff = 1.0 / (0.001 + lightDistance);
				
				float pulse = cos(_Time * 25) * 0.5 + 0.5;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex 
					+ float4(v.normal * (1 - saturate( lightFalloff * 5 ) ) * pulse, 0) );
					
				//o.color = v.normal * 0.5 + 0.5;
				o.color = lerp(float3(0.7,0.2,0.4), float3(1,1,0.1), lightFalloff.xxx);
				o.color *= length(v.vertex.xyz) * 0.1;
				
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
