Shader "Custom/Distort"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UpPower("UpPower", Float) = 1
        _SideSpeed("SideSpeed", Float) = 1
        _SideStrength("SideStrength", Float) = 1
        _SideYPower("SideYPower", Float) = 1
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
            float _UpPower;
            float _SideSpeed;
            float _SideStrength;
            float _SideYPower;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float upModifier = sqrt(2 * abs(i.uv.y - 0.5f)) * _UpPower;
                fixed4 colUp = tex2D(_MainTex, float2(i.uv.x, sign(i.uv.y - 0.5f) * upModifier / 2 + 0.5f));
                float sideModifier = pow(abs(i.uv.x - 0.5f), 2);
                fixed4 colSide = tex2D(_MainTex, float2(i.uv.x + sin(_Time[1] * _SideSpeed + i.uv.y * _SideYPower) * _SideStrength * sideModifier * sign(i.uv.x - 0.5f), i.uv.y));
                fixed4 result = (1 - upModifier / 1.5f - sideModifier / 1.5f) * col + upModifier * colUp / 2 + colSide * sideModifier;
                return result;
            }
            ENDCG
        }
    }
}
