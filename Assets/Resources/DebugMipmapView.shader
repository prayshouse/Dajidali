Shader "Debug/MipmapView" {
	Properties {
		_mainTexture ("mainTexture", 2D) = "white" {}
	}
	SubShader {
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.0

			struct v2f {
				float4 pos : SV_POSITION;
				float2 tc : TEXCOORD0;
			};

			sampler2D _mainTexture;
			float4 _mainTexture_TexelSize;

			v2f vert(float4 vertex : POSITION, float2 tc : TEXCOORD0)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.tc = tc;
				return o;
			}

			half4 frag(v2f i) : COLOR0
			{
				float2 textrueCoord = i.tc * _mainTexture_TexelSize.zw;
				float2 dx_vtc = ddx(textrueCoord);
				float2 dy_vtc = ddy(textrueCoord);
				float delta_max_sqr = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
				half mipmapLevel = max(0, 0.5 * log2(delta_max_sqr));

				int maxLength = max(_mainTexture_TexelSize.z, _mainTexture_TexelSize.w);
				half textureLevel = 12.0 - log2(maxLength);

				half weight;
				half4 mipColor;
				half4 color = tex2D(_mainTexture, i.tc);

				half4 res;
				if (textureLevel > mipmapLevel)
				{
					weight = textureLevel - mipmapLevel;
					mipColor = half4(0, 0, 1, 0);
				}
				else
				{
					weight = mipmapLevel - textureLevel;
					mipColor = half4(1, 0, 0, 0);
				}
				if (weight > 5)
					res = mipColor;
				else
					res = lerp(color, mipColor, weight / 5.0);
				// res = half4(1, 1, 1, 0);
				return res;    
			}
			ENDCG
		}
	}
}
