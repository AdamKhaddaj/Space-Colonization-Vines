Shader "Unlit/DrawLeaf"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LeafTex("Leaf Texture", 2D) = "black" {}
        _Position("Leaf Position", Vector) = (0.0, 0.0, 0.0, 0.0)
        _Scale("Leaf Scale", Vector) = (0.05, 0.05, 0.0, 0.0)
        _Rotation("Leaf Rotation", float) = 25.0
        _PositionOffset("Leaf Position Offset", float) = 0.0
        _BrightnessVariance("Brightness Variance", Range(0.0, 2.0)) = 1.0
        _ColorVariance("Variance Color", Vector) = (1.0, 1.0, 1.0, 1.0)
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
            sampler2D _LeafTex;
            float4 _MainTex_ST;

            fixed4 _Position;
            fixed4 _ColorVariance;
            float _PositionOffset;
            fixed4 _Scale;
            float _Rotation;
            float _BrightnessVariance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 translate(float2 position, float2 translation)
            {

                //create trans matrix
                float3x3 transMat = float3x3 (
                    1.0, 0.0, translation.x,
                    0.0, 1.0, translation.y,
                    0.0, 0.0, 1.0
                    );

                //translate and return position using rotation matrix
                return mul(transMat, fixed3(position, 1.0)).xy;
            }

            float2 rotate(float2 position, float angleInDegrees)
            {
                float PI = 3.14159265;

                //convert degrees to radians
                float angleInRad = angleInDegrees * PI / 180;

                //create rotation matrix
                float2x2 rotMat = float2x2 (
                    cos(angleInRad), -sin(angleInRad),
                    sin(angleInRad), cos(angleInRad)
                    );

                //rotate and return position
                //translating so rotation is around origin at center of the image
                float2 res = translate(position, float2(-0.5, -0.5));
                res = mul(rotMat, res);
                res = translate(res, float2(0.5, 0.5));

                return res;
            }

            float2 scale(float2 position, float2 scale)
            {

                //create scale matrix
                float3x3 scaleMat = float3x3 (
                    scale.x, 0.0, 0.0,
                    0.0, scale.y, 0.0,
                    0.0, 0.0, 1.0
                    );

                //scale
                float2 res = translate(position, float2(-0.5, -0.5));
                res = mul(scaleMat, res);
                res = translate(res, float2(0.5, 0.5));

                return res;
            }

            fixed4 frag(v2f i) : SV_Target
            {
 
                fixed2 pos;
            
                _Position.y = 1 - _Position.y;

                _PositionOffset -= 0.5;
                _Position.xy -= 0.5;
                
                //transform position of leaf to be placed
                pos = scale(i.uv, fixed2(_Scale.x, _Scale.y));
                pos = translate(pos, _Position.xy * -_Scale);
                pos = rotate(pos, _Rotation);
                pos = translate(pos, fixed2(0.0, 0.5 + _PositionOffset) * -_Scale);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 leafCol = tex2D(_LeafTex, pos);

                _BrightnessVariance = lerp(0.8, 1.2, _BrightnessVariance);

                //vary leaf color
                leafCol.xyz *= _BrightnessVariance; // *_ColorVariance.xyz;

                fixed4 res = leafCol;

                if (leafCol.a == 0.0)
                {
                    res = col;
                }

                return res;
            }
            ENDCG
        }
    }
}
