Shader "Custom/Paintable Surface"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MaskTexture ("Mask Texture", 2D) = "black" {}
        _NoiseSample ("Noise Texture", 2D) = "white" {}
    }
    SubShader
    {
        // Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        // BlendOp Max 
        // Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : POSITION1;
                float3 worldPos : POSITION2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTexture;
            float4 _MaskTexture_ST;
            sampler2D _NoiseSample;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                // fixed4 col = fixed4(0,0,0,0);
                float3 worldPos = i.worldPos;
                float3 normal = i.normal;

                float3 blends = abs(normal);
                blends /= blends.x + blends.y + blends.z;

                float3 projX = tex2D(_MainTex, worldPos.yz) * blends.x;
                float3 projY = tex2D(_MainTex, worldPos.xz) * blends.y;
                float3 projZ = tex2D(_MainTex, worldPos.xy) * blends.z;

                fixed4 col = fixed4((projX + projY + projZ), 0);
                col *= _Color;

                // Filter mask
                fixed4 maskCol = tex2D(_MaskTexture, i.uv);
                float oneMinusAlpha = 1 - maskCol.a;

                float blendValue = maskCol.a * tex2D(_NoiseSample, i.uv).r;
                blendValue += maskCol.a;

                float maskBlendFactor = step(oneMinusAlpha, blendValue);


                fixed4 finalCol = lerp(col, maskCol, maskBlendFactor);
                return finalCol;
            }
            ENDCG
        }
    }
}
