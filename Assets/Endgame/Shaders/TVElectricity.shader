// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TVElectricity"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseSpeed("Noise Speed", float) = 1
        _Offset("Offset", Range(0, 1)) = 0.5
        _GlobalOffset ("Global Offset", Range(0, 1)) = 0.5
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _NoiseSpeed;
            float _Offset;
            float _GlobalOffset;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float alpha = _Time[1] * _NoiseSpeed;
                float co = cos(alpha);
                float si = sin(alpha);
                float2 pos = float2(i.screenPos[0] / i.screenPos.w, i.screenPos[1] / i.screenPos.w);
                float2 temp = float2((pos[0] - _Offset) * co + _Offset + (pos[1] - _Offset) * si + _Offset, -(pos[0] - _Offset) * si - _Offset + (pos[1] - _Offset) * co - _Offset);
                temp += float2(_GlobalOffset, _GlobalOffset);
                col *= tex2D(_NoiseTex, temp) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
