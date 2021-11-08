Shader "Custom/PostProcess"
{
    Properties
    {
        _MainTex ("Nothing", 2D) = "white" {}
        _RenderTex ("Render Texture", 2D) = "white" {}
		_SizeX ("SizeX", Int) = 256
        _SizeY ("SizeY", Int) = 240
		[Toggle] _Stretch ("Stretch", Int) = 0
		[Toggle] _Filter ("Filter", Int) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _RenderTex;
            float _SizeX;
            float _SizeY;
			float _Stretch;
			float _Filter;

            fixed4 frag(v2f i) : SV_Target
            {
                // Find screen size
                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);
				// Find size mod
				float sizeMod;
				if (_Stretch)
				{
					sizeMod = min(screenSize.x / _SizeX, screenSize.y / _SizeY);
				}
				else
				{
					sizeMod = min(floor(screenSize.x / _SizeX), floor(screenSize.y / _SizeY));
				}
				// Find pixel pos
				float2 pixelPos = float2(i.uv.x * screenSize.x - floor((screenSize.x - _SizeX * sizeMod) / 2), i.uv.y * screenSize.y - floor((screenSize.y - _SizeY * sizeMod) / 2));
				float2 finalPos = float2(floor(pixelPos.x / sizeMod) + 0.5f, floor(pixelPos.y / sizeMod) + 0.5f);
				// If in range, get same pixel from RenderTex
				if (finalPos.x >= 0 && finalPos.x < _SizeX && finalPos.y >= 0 && finalPos.y < _SizeY)
				{
					if (_Filter)
					{
						// Find how close it's to the center of the pixel
						float2 relativeToCenter = float2(pixelPos.x % (floor(pixelPos.x / sizeMod) * sizeMod) - (sizeMod - 1) / 2, pixelPos.y % (floor(pixelPos.y / sizeMod) * sizeMod) - (sizeMod - 1) / 2);
						// Weigh each nearby pixel, and find the most common colour
						float colourWeights[9];
						fixed4 colours[9];
						int k;
						for (k = 0; k < 9; k++)
						{
							colours[k] = fixed4(0,0,0,1);
							colourWeights[k] = 0;
						}
						for (int i = -1; i <= 1; i++)
						{
							for (int j = -1; j <= 1; j++)
							{
								float weight = min(1, 1 - min(1, sqrt(pow(relativeToCenter.x / sizeMod - i, 2) + pow(relativeToCenter.y / sizeMod - j, 2)) * 2));
								if (weight > 0)
								{
									float2 fixedRelative = float2(floor((pixelPos.x + i) / sizeMod) + 0.5f, floor((pixelPos.y + j) / sizeMod) + 0.5f);
									fixed4 colour = tex2D(_RenderTex, fixedRelative);
									for (k = 0; k < 9; k++)
									{
										if (all(colours[k].rgb == colour.rgb))
										{
											colourWeights[k] += weight;
											break;
										}
										else if (all(colours[k].rgb == fixed4(0,0,0,1).rgb))
										{
											colours[k] = colour;
											colourWeights[k] += weight;
											break;
										}
									}
								}
							}
						}
						fixed4 max = fixed4(0,0,0,1);
						float maxValue = 0;
						for (k = 0; k < 9; k++)
						{
							if (all(colours[k].rgb == fixed4(0,0,0,1).rgb))
							{
								break;
							}
							else if (colourWeights[k] > maxValue)
							{
								max = colours[k];
							}
						}
						return max;
					}
					else
					{
						// Find relative pos in RenderTex
						float2 fixedRelative = float2(finalPos.x / _SizeX, finalPos.y / _SizeY);
						return tex2D(_RenderTex, fixedRelative);
					}
				}
                return fixed4(0,0,0,1);
            }
            ENDCG
        }
    }
}
