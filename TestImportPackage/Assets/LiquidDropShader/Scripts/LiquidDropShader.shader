Shader "WaterEffect/WaterMeshAnimator"
{
    Properties
    {
        _MainTex("Main Tex", 2D) = "white" {}
        _BaseUVs("Base UVs", 2D) = "white" {}
        _FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        _InvertedFresnelColor("Inverted Fresnel Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalRenderPipeline" 
            "UniversalMaterialType" = "Lit"
            "Queue" = "Geometry"
            }


        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;          
                float3 normal : NORMAL;     
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                float2 baseUVs : TEXCOORD5;
            };

            float _FlowCutoff;
            float _WobbleSpeed;
            float _ReverseDrop;
            float _FresnelIntensity;
            float _FresnelRamp;
            float _InvertedFresnelIntensity;
            float _InvertedFresnelRamp;
            float4 _FresnelColor, _InvertedFresnelColor;
            float _UVScaler;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BaseUVs);
            SAMPLER(sampler_BaseUVs);

            CBUFFER_START(UnityPerMaterial) 
                float4 _MainTex_ST;
                float4 _BaseUVs_ST;
            CBUFFER_END 

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                //Vertex Displacement to cause mesh to "wobble"
                float originalPos = IN.positionOS.x;
                float newPos = -abs(originalPos * sin(_Time.y * _WobbleSpeed));

                //Only apply changes to mesh if it causes it to go "further away" than original mesh position
                if(newPos > originalPos) {
                    IN.positionOS.x = -abs((newPos)/7 + originalPos);
                }
                
                //Get vertex positions
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;

                //Get values for fresnel functions in frag
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal.xyz);
                OUT.viewDir = GetWorldSpaceNormalizeViewDir(OUT.positionWS);

                //Sample textures
                // "_BaseUVS" is a constant texture that looks at regular mesh UVs (_MainText uses UV displacement for Wave Animation)
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.baseUVs = TRANSFORM_TEX(IN.uv, _BaseUVs);

                //Displaces UVs for animation
                OUT.uv -= float4(0,_UVScaler,0,0) * _Time.y;
                _FlowCutoff += _UVScaler * _Time.y;

                return OUT;
            }
          
            half4 frag(Varyings IN) : SV_Target
            {
                //Hides portions of the mesh for the drop in/drop out liquid animation
                //_ReverseDrop == 1 is used for Drop Animation, 0 to stop pouring.
                if(_ReverseDrop == 0) {
                    clip(-_FlowCutoff + IN.baseUVs.y);
                }
                else {
                    clip(_FlowCutoff - IN.baseUVs.y);
                }
                //Rim Color for liquid
                float fresnel = 1 - max(0,dot(IN.normalWS, IN.viewDir));
                fresnel = pow(fresnel, _FresnelRamp) * _FresnelIntensity;

                //Main Liquid Color effect with reverse fresnel
                float invertedFresnel = max(0,dot(IN.normalWS, IN.viewDir));
                invertedFresnel = pow(invertedFresnel, _InvertedFresnelRamp) * _InvertedFresnelIntensity;

                half4 textSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                //Applies color and combines both fresnel effects
                float4 fresnelColor = _FresnelColor * fresnel;
                float4 invertedFresnelColor = _InvertedFresnelColor *invertedFresnel;
                float4 color = fresnelColor + invertedFresnelColor;

                //Makes wave texture color match rim liquid (fresnel) color
                if(textSample.r >= 0.3) {
                    textSample *= _FresnelColor;
                    color += textSample; 
                }
                return color;
            }
            ENDHLSL
        }
    }
}
