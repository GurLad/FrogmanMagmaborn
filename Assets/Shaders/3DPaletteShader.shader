Shader "Custom/3DPaletteShader"
{
    Properties
    {
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
        _Color1out("Color 1 Out", Color) = (1,1,1,1)
        _Color1out("Color 1 Out", Color) = (1,1,1,1)
        _Color2out("Color 2 Out", Color) = (1,1,1,1)
        _Color3out("Color 3 Out", Color) = (1,1,1,1)
        _Color4out("Color 4 Out", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert finalcolor:mycolor
        struct Input {
            float2 uv_MainTex;
        };
        fixed4 _ColorTint;
        fixed4 _Color1out;
        fixed4 _Color2out;
        fixed4 _Color3out;
        fixed4 _Color4out;
        void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
        {
            color *= _ColorTint;
            // Apperantly, colors can exceed 1 for no apperant reason
            if (color.r >= 1)
            {
                color.r = 1;
            }
            if (color.g >= 1)
            {
                color.g = 1;
            }
            if (color.b >= 1)
            {
                color.b = 1;
            }
            // Apply palette
            float brightness = color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
            int brightnessLevel = floor((brightness - 0.00001) * 4);
            if (brightnessLevel == 3)
            {
                color = _Color1out;
            }
            else if (brightnessLevel == 2)
            {
                color = _Color2out;
            }
            else if (brightnessLevel == 1)
            {
                color = _Color3out;
            }
            else
            {
                color = _Color4out;
            }
        }
        sampler2D _MainTex;
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    } 
    FallBack "Diffuse"
}
