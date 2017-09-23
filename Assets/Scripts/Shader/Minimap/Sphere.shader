Shader "Minimap/simpleColor"
{
	Properties
	{
		_MainColor ("Color", COLOR) = (0.0, 0.0, 0.0, 0.0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			fixed4 _MainColor;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _MainColor;
			}
			ENDCG
		}
	}
}
