// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "TerraUnity/Analysis Projector"
{ 
	Properties
    {
		_Color ("Main Color", Color) = (0.25, 0.25, 0.25, 1.0)
		_ShadowTex ("Shadow Map", 2D) = "gray" {}
        _FalloffTex ("FallOff", 2D) = "white" {}
        _Power ("Power", Range (0.01, 4.0) ) = 2.0
        _ScaleX ("Scale X", Range (0.0, 10.0) ) = 1.0
        _ScaleY ("Scale Y", Range (0.0, 10.0) ) = 1.0
	}
	
	Subshader
    {
		Pass
        {
			ZWrite Off
			ColorMask RGB
	 		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f
            {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
                float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
                o.pos = UnityObjectToClipPos (vertex);
                o.uvShadow = mul (unity_Projector, vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				return o;
			}
			
			fixed4 _Color;
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
            float _Power;
            float _ScaleX;
            float _ScaleY;
			
			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
                //fixed alpha = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow)).a;
                fixed alpha = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(float4(i.uvShadow.x * _ScaleX, i.uvShadow.y * _ScaleY, i.uvShadow.z, i.uvShadow.w))).a;

                fixed4 falloff = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));

				//col += _Color.rgb;
                fixed4 col = alpha + _Color.rgba;

                col *= falloff.a;
                col *= _Power;
				
				return col;
			}
			ENDCG
		}
	}
}

