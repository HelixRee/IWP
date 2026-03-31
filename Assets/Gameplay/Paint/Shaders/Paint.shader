Shader "Custom/Paint"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        
    }
    SubShader
    {
        // Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        // BlendOp Add
        // Blend One One
        
        Cull Off ZWrite Off ZTest Off

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            // Texture
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Paint
            float3 _PainterPosition;
            float4 _PainterColor;
            float _Radius;
            float _Hardness;
            float _Strength;

            float mask(float3 position, float3 center, float radius, float hardness)
            {
                float m = distance(center, position);
                return 1 - smoothstep(radius * hardness, radius, m);
            }

            v2f vert (appdata v)
            {
                v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
				float4 uv = float4(0, 0, 0, 1);
                // uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2( 2, 2) - float2(1, 1));
                uv.xy = (v.uv.xy * 2 - 1) * float2(1, _ProjectionParams.x);
				o.vertex = uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                // float4 col = float4(0, 0, 0, 0);
                float m = mask(i.worldPos, _PainterPosition, _Radius, _Hardness);
                float edge = m * _Strength;
                return lerp(col, _PainterColor, edge);
            }

            ENDCG
        }
    }
}
