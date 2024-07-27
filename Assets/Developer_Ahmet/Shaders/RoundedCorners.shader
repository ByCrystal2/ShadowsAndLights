Shader "UI/RoundedCorners"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Overlay-1" "RenderType"="Transparent" }  // Queue'yu "Overlay-1" olarak ayarladýk
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.texcoord);
                float2 uv = i.texcoord * 2.0 - 1.0;
                float2 absUV = abs(uv) - (1.0 - _Radius);
                float dist = length(max(absUV, 0.0)) - _Radius;
                color.a *= smoothstep(0.0, fwidth(dist), dist);
                return color;
            }
            ENDCG
        }
    }
}
