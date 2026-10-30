Shader "Custom/CandleFlame"
{
    Properties
    {
        _MainTex ("Flame Texture (Alpha Mask)", 2D) = "white" {}
        _ColorInner ("Inner Flame Color", Color) = (1, 0.9, 0.5, 1)
        _ColorOuter ("Outer Flame Color", Color) = (1, 0.2, 0, 1)
        _FlickerSpeed ("Flicker Speed", Float) = 15.0
        _FlickerStrength ("Flicker Strength", Range(0, 0.1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off
        Blend One One // Additive blending for that "glow" look
        Cull Off      // Make sure we see it from both sides

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

            sampler2D _MainTex;
            float4 _ColorInner;
            float4 _ColorOuter;
            float _FlickerSpeed;
            float _FlickerStrength;

            v2f vert (appdata v)
            {
                v2f o;
                
                float flicker = sin(_Time.y * _FlickerSpeed) * v.uv.y * _FlickerStrength;
                v.vertex.x += flicker;
                v.vertex.y += flicker * 0.5; // Slight vertical stretch

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Create a vertical gradient for color
                // Bottom is Inner Color, Top is Outer Color
                fixed4 flameColor = lerp(_ColorInner, _ColorOuter, i.uv.y);
                
                return col * flameColor * 2.0; 
            }
            ENDCG
        }
    }
}