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
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            // #include "UnityCG.cginc"

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
                float4 shadowCoords : TEXCOORD3;
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
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                
                // Get the VertexPositionInputs for the vertex position  
                VertexPositionInputs positions = GetVertexPositionInputs(v.vertex.xyz);

                // Convert the vertex position to a position on the shadow map
                float4 shadowCoordinates = GetShadowCoord(positions);

                // Pass the shadow coordinates to the fragment shader
                o.shadowCoords = shadowCoordinates;


                return o;
            }

            float4 frag (v2f i) : SV_Target
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

                float4 col = float4((projX + projY + projZ), 0);
                col *= _Color;

                // Filter mask
                float4 maskCol = tex2D(_MaskTexture, i.uv);
                float oneMinusAlpha = 1 - maskCol.a;

                float blendValue = maskCol.a * tex2D(_NoiseSample, i.uv).r;
                blendValue += maskCol.a;

                float maskBlendFactor = step(oneMinusAlpha, blendValue);


                float4 finalCol = lerp(col, maskCol, maskBlendFactor);


                                // Get the value from the shadow map at the shadow coordinates
                half shadowAmount = MainLightRealtimeShadow(i.shadowCoords);





                return finalCol;
            }
            ENDHLSL
        }
    }
}
