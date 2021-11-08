Shader "Custom/PostProcess"
{
    Properties
    {
        _MainTex ("Nothing", 2D) = "white" {}
        _RenderTex ("Render Texture", 2D) = "white" {}
		_SizeX ("SizeX", Int) = 256
        _SizeY ("SizeY", Int) = 240
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

            fixed4 frag(v2f i) : SV_Target
            {
                // Find screen size
                float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);
				// Find size mod
				int sizeMod = min(floor(screenSize.x / _SizeX), floor(screenSize.y / _SizeY));
				// Find pixel pos
				float2 pixelPos = float2(floor((i.uv.x * screenSize.x) / sizeMod) + 0.5f, floor((i.uv.y * screenSize.y) / sizeMod) + 0.5f);
				// If in range, get same pixel from RenderTex
				if (pixelPos.x >= 0 && pixelPos.x < _SizeX && pixelPos.y >= 0 && pixelPos.y < _SizeY)
				{
					// Find relative pos in RenderTex
					float2 fixedRelative = float2(pixelPos.x / _SizeX, pixelPos.y / _SizeY);
					return tex2D(_RenderTex, fixedRelative);
				}
                return fixed4(0,0,0,1);
            }
            ENDCG
        }
    }
}
