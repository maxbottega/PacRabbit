Shader "Custom/BlobbySphereShiny" 
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

			// --- Shader code
//			static const float3 BunchOfPoints[3] =
//			{ 
//				float3(-2.54, 5, -8.4)
//				,float3(-3.54, 4.7, -8.8)
//				,float3(-3, 2.7, -8.5)
//			};

			#define SIGNED_TIME_PULSE (cos(_Time*10))
			#define SIGNED_TIME_PULSE_X(x) (cos(_Time*10*x))
			#define TIME_PULSE (cos(_Time*10)*0.5+0.5)
			#define TIME_PULSE_X(x) (cos(_Time*10*x)*0.5+0.5)

			float DistanceEstimator(float3 worldPoint)
			{
				float3 OBJCENTER = float3(0,0,0);
				float OBJSCALE = 0.7;

				float3 worldPointMod = worldPoint;
				float d = length(worldPointMod - OBJCENTER) - OBJSCALE;
				float sc = 7;
				d += (sin(sc*worldPoint.x) * sin(sc*worldPoint.y) * sin(sc*worldPoint.z)) * (SIGNED_TIME_PULSE*0.2);
				sc = 11;
				d += (sin(sc*worldPoint.x) * sin(sc*worldPoint.y) * sin(sc*worldPoint.z)) * (SIGNED_TIME_PULSE_X(1.7)*0.05);
				sc = 3;
				d += (sin(sc*worldPoint.x) * sin(sc*worldPoint.y) * sin(sc*worldPoint.z)) * (SIGNED_TIME_PULSE_X(1.3)*0.2);
		
//				float d = distance(BunchOfPoints[0], worldPoint) - (0.3+TIME_PULSE_X(3)*0.3);
//				d = min(d,distance(BunchOfPoints[1], worldPoint) - (0.1+TIME_PULSE_X(7)*0.1));
//				float sc = 7;
//				d += (sin(sc*worldPoint.x) * sin(sc*worldPoint.y) * sin(sc*worldPoint.z)) * (SIGNED_TIME_PULSE*0.2);
//				d = min(d,dot(float4(worldPoint,1), float4(0,1,0,-2.5)));
//				sc = 11;
//				d += (sin(sc*worldPoint.x) * sin(sc*worldPoint.y) * sin(sc*worldPoint.z)) * (SIGNED_TIME_PULSE_X(1.7)*0.05);
//				d *= min(distance(BunchOfPoints[2]+float3(SIGNED_TIME_PULSE,0,0), worldPoint ) - 1.5, 3);
//				sc = 3;
//				d += (sin(sc*worldPoint.x) * sin(sc*worldPoint.y) * sin(sc*worldPoint.z)) * (SIGNED_TIME_PULSE_X(1.3)*0.2);
				
				return d;
			}

			 // rayDir has to be normalized
			float RayMarchDistanceField(
				float3 rayOrig, float3 rayDir, float maxTravel, out float3 hitPoint, out float3 hitNormal
			)
			{
				float distanceTravelled = 0.0;
				float distance;
				
				const int MAX_RAY_STEPS = 20;
				const float MIN_SURFACE_DISTANCE = 0.05;
				
				for (int steps=0; steps < MAX_RAY_STEPS; steps++) 
				{
					hitPoint = rayOrig + distanceTravelled * rayDir;
					distance = DistanceEstimator(hitPoint);
					
//					distance = min(distance, 1.f/(1.f+steps));
//					distance = min(distance, 0.5*sign(distance));
					distanceTravelled += distance; //* (1.f + 2.f/(1.f+steps)); //"overrelaxation"
					// The "max" is optional but it avoids finding intersections past the ray origin
//					distanceTravelled = max(distanceTravelled, 0);	
			
//					 This following check disallows backstepping and will "fill" holes
//					if (distance < MIN_SURFACE_DISTANCE) break; //As an optimization a dynamic branch could be used every N steps
//					if(distanceTravelled > maxTravel) break;//As an optimization a dynamic branch could be used every N steps
				}
				
				const float2 NORM_EPS = {0.1, 0};
//				hitNormal = // finite difference estimator
//					normalize(float3(
//						DistanceEstimator(hitPoint+NORM_EPS.xyy) - DistanceEstimator(hitPoint-NORM_EPS.xyy),
//						DistanceEstimator(hitPoint+NORM_EPS.yxy) - DistanceEstimator(hitPoint-NORM_EPS.yxy),
//						DistanceEstimator(hitPoint+NORM_EPS.yyx) - DistanceEstimator(hitPoint-NORM_EPS.yyx)
//					));
					
				hitNormal = // finite difference estimator, de(x) - de(x+eps) assuming de(x)==0 as we're near the surface
					normalize(float3(
						- DistanceEstimator(hitPoint-NORM_EPS.xyy),
						- DistanceEstimator(hitPoint-NORM_EPS.yxy),
						- DistanceEstimator(hitPoint-NORM_EPS.yyx)
					));
					
				return distanceTravelled;
			}
			
			struct vertexOut 
			{
				float4 pos : SV_POSITION;
				float color : COLOR0;
				float3 wnormal : TEXCOORD0;
				float3 wpos : TEXCOORD1;
			};
			vertexOut vs (appdata_base v) // http://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
			{
				vertexOut o;
				//float3 objectSpaceLPos = mul(unity_LightPosition[0],UNITY_MATRIX_IT_MV); 
				//float lightDistance = distance(v.vertex.xyz, objectSpaceLPos);
				
				// NOTE --- input geometry is assumed to be a unit sphere at zero
				float3 worldPos = normalize(v.vertex.xyz); //mul (UNITY_MATRIX_M, v.vertex);
				float3 worldNormal = -worldPos; //mul(UNITY_MATRIX_IT_M, v.normal);
				
				float3 hitPoint, hitNormal;
				float dist = RayMarchDistanceField(
					worldPos, worldNormal, 1, hitPoint, hitNormal
				);
				
				//hitPoint *= 1 - (length(v.vertex.xyz)-1) * 0.1;
				
				o.color = dist.x; // * (hitNormal.y*0.5+0.5);
				o.wpos = mul (UNITY_MATRIX_MV, float4(hitPoint, 1)); 
				o.pos = mul (UNITY_MATRIX_MVP, float4(hitPoint, 1)); // UNITY_MATRIX_VP doesn't work in win?!?
				o.wnormal = hitNormal;
				
				return o;
			}
			
			half4 ps (vertexOut i) : COLOR
			{
				float3 view = normalize(_WorldSpaceCameraPos - float3(i.wpos));
				float3 norm = normalize(i.wnormal);
			    float3 refl = reflect(view, norm);
				float fresnel = saturate(pow(1 - dot(view, norm), 4) + 0.01);
			
				float4 rgbm = texCUBElod(_SpecularRadianceTex, float4(refl, i.color * 7) );
				float3 spec = rgbm.rgb * rgbm.a * 8;
				rgbm = texCUBElod(_SpecularRadianceTex, float4(norm, 7) );
				float3 diff = rgbm.rgb * rgbm.a * 8;
				
				float3 col = spec * fresnel * i.color + diff * (1-fresnel) * float3(0.1, 0, 0);
				
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
