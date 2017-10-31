Shader "Debug/MipmapLevelView"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Level10("Level10 (1 * 1)", COLOR) = (1.0, 0.0, 0.0, 0.0)
		_Level9("Level9  (2 * 2)", COLOR) = (1.0, 0.4, 0.0, 0.0)
		_Level8("Level8  (4 * 4)", COLOR) = (1.0, 0.8, 0.0, 0.0)
		_Level7("Level7  (8 * 8)", COLOR) = (0.8, 1.0, 0.0, 0.0)
		_Level6("Level6  (16 * 16)", COLOR) = (0.4, 1.0, 0.0, 0.0)
		_Level5("Level5  (32 * 32)", COLOR) = (0.0, 1.0, 0.0, 0.0)
		_Level4("Level4  (64 * 64)", COLOR) = (0.0, 1.0, 0.4, 0.0)
		_Level3("Level3  (128 * 128)", COLOR) = (0.0, 1.0, 0.8, 0.0)
		_Level2("Level2  (256 * 256)", COLOR) = (0.0, 0.8, 1.0, 0.0)
		_Level1("Level1  (512 * 512)", COLOR) = (0.0, 0.4, 1.0, 0.0)
		_Level0("Level0  (1024 * 1024)", COLOR) = (0.0, 0.0, 1.0, 0.0)
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct v2f
			{
				float2 tc : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			half4 _Level0;
			half4 _Level1;
			half4 _Level2;
			half4 _Level3;
			half4 _Level4;
			half4 _Level5;
			half4 _Level6;
			half4 _Level7;
			half4 _Level8;
			half4 _Level9;
			half4 _Level10;

			v2f vert(float4 vertex : POSITION, float2 tc : TEXCOORD0)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.tc = tc;
				return o;
			}

			half4 frag(v2f i) : COLOR0
			{
				float2 textrueCoord = i.tc * _MainTex_TexelSize.zw;
				float2 dx_vtc = ddx(textrueCoord);
				float2 dy_vtc = ddy(textrueCoord);
				float delta_max_sqr = min(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
				half mipmapLevel = max(0, 0.5 * log2(delta_max_sqr));

				half4 res;

				_Level0 = half4(0.0, 0.0, 1.0, 0.0);
				_Level1 = half4(0.0, 0.4, 1.0, 0.0);
				_Level2 = half4(0.0, 0.8, 1.0, 0.0);
				_Level3 = half4(0.0, 1.0, 0.8, 0.0);
				_Level4 = half4(0.0, 1.0, 0.4, 0.0);
				_Level5 = half4(0.0, 1.0, 0.0, 0.0);
				_Level6 = half4(0.4, 1.0, 0.0, 0.0);
				_Level7 = half4(0.8, 1.0, 0.0, 0.0);
				_Level8 = half4(1.0, 0.8, 0.0, 0.0);
				_Level9 = half4(1.0, 0.4, 0.0, 0.0);
				_Level10 = half4(1.0, 0.0, 0.0, 0.0);

				if (mipmapLevel < 1)
					res = lerp(_Level0, _Level1, mipmapLevel - 0.0);
				else if (mipmapLevel < 2)
					res = lerp(_Level1, _Level2, mipmapLevel - 1.0);
				else if (mipmapLevel < 3)
					res = lerp(_Level2, _Level3, mipmapLevel - 2.0);
				else if (mipmapLevel < 4)
					res = lerp(_Level3, _Level4, mipmapLevel - 3.0);
				else if (mipmapLevel < 5)
					res = lerp(_Level4, _Level5, mipmapLevel - 4.0);
				else if (mipmapLevel < 6)
					res = lerp(_Level5, _Level6, mipmapLevel - 5.0);
				else if (mipmapLevel < 7)
					res = lerp(_Level6, _Level7, mipmapLevel - 6.0);
				else if (mipmapLevel < 8)
					res = lerp(_Level7, _Level8, mipmapLevel - 7.0);
				else if (mipmapLevel < 9)
					res = lerp(_Level8, _Level9, mipmapLevel - 8.0);
				else if (mipmapLevel < 10)
					res = lerp(_Level9, _Level10, mipmapLevel - 9.0);
				else
					res = _Level10;
				return res;
			}
			ENDCG
		}
	}
}
