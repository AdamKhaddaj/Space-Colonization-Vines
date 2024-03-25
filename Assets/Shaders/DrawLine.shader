Shader "Unlit/DrawLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StartPosition("Start Position", Vector) = (0.0, 0.0, 0.0)
        _EndPosition("End Position", Vector) = (0.0, 0.0, 0.0)
        _Color("Color", Color) = (1.0, 0.0, 0.0, 0.0)
        _Thickness("Line Thickness", float) = 0.005
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
            float4 _MainTex_ST;

            float2 _StartPosition;
            float2 _EndPosition;
            fixed4 _Color;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                //algorithm to draw a line from https://www.shadertoy.com/view/MlcGDB
                float2 startPos = _StartPosition.xy;
                float2 endPos = _EndPosition.xy;
                
                startPos.y = 1 - startPos.y;
                endPos.y = 1 - endPos.y;

                float2 g = endPos - startPos;
                float2 h = i.uv - startPos;
                float d = length(h - g * clamp(dot(g, h) / dot(g, g), 0.0, 1.0));
                float draw = smoothstep(_Thickness, 0.5 * _Thickness, d);

                fixed4 drawColor = (_Color * draw);

                if (col.r != 0.0 && col.g != 0.0 && col.b != 0.0 && col.a != 0.0)
                    drawColor = col;

                return drawColor;
            }
            ENDCG
        }
    }
}
