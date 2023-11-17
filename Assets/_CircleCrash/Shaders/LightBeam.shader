Shader "SgLib/LightBeam" 
{
		Properties 
		{
			_Color ("Color", Color) = (1,1,1,1)
			_Intensity("Intensity", Float ) = 12
			_FadeBias("Fade Bias", Float) = 30
		}

		SubShader 
		{
			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
			LOD 300


			Pass
			{
				Tags { "LightMode" = "ForwardBase" }

				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off    

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				fixed4 _Color;
				float _Intensity;
				float _FadeBias;

				struct appdata
				{
					float4 pos : POSITION;
					float3 normal : NORMAL;
				};

				struct v2f 
				{
					float4 clipPos : SV_POSITION;
					float4 worldPos : TEXCOORD0;
					float3 worldNormal : NORMAL;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.clipPos = UnityObjectToClipPos(v.pos);
					o.worldPos = mul(unity_ObjectToWorld, v.pos);
					o.worldNormal = UnityObjectToWorldNormal(v.normal);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float3 meshWorldPos;
					meshWorldPos.x = unity_ObjectToWorld[0].w;
					meshWorldPos.y = unity_ObjectToWorld[1].w;
					meshWorldPos.z = unity_ObjectToWorld[2].w;

					float dist = length(i.worldPos.xyz - meshWorldPos);
					float fade = 1 - saturate(dist / _Intensity);

					fade = pow(fade, _FadeBias) * 10;

					return fixed4(_Color.rgb, _Color.a * fade);
				}

				ENDCG
			}
	}
	Fallback "Transparent/VertexLit"
}
