Shader "VFX/Character/ToonTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _AmbientLightColor ("Ambient Light Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _AmbientCoefficient ("Ambient Coefficient", Float) = 0.05
        _Glossiness ("Glossiness", Float) = 100
        _EdgeThreshold ("Edge Threshold", Range(0.0, 1)) = 0.01
    }
    SubShader
    {
        Tags
        { 
            "RenderType"="Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
        HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" // To get the main light
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/use-built-in-shader-methods-lighting.html

            #pragma vertex      vert
            #pragma fragment    frag

            Light light;
            
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _AmbientLightColor;
                float _AmbientCoefficient;
                float _Glossiness;
                float _EdgeThreshold;
            CBUFFER_END

            struct MeshData
            {
                float4 vertex   : POSITION;
                float3 normal   : NORMAL;
                float2 uv       : TEXCOORD0;
            };

            struct FragmentData
            {
                float4 position     : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normal       : NORMAL;
                //half4  lightColor   : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
            };

            FragmentData vert(MeshData input)
            {
                FragmentData output;

                output.position     = TransformObjectToHClip(input.vertex.xyz); // apply MVP matrix, from 3D world to the viewport
                output.normal       = normalize(input.normal);                  // normalize normals (the interpolated ones)
                output.uv           = input.uv;                                 // Pass UV directly if we do not have a sampler
                output.positionWS   = GetVertexPositionInputs(input.vertex.xyz).positionWS;
                // Ref https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/use-built-in-shader-methods-transformations.html
                
                // Note that computing the light model on the vertex shader will yield
                // facetaded shading, due to the interpolation
                //output.lightColor = half4(colorWithAmbient);
                return output;
            };

            float4 frag(FragmentData fragData) : SV_Target
            {

                Light light = GetMainLight();

                // Simplified Lambert
                VertexNormalInputs fragNInputs = GetVertexNormalInputs(fragData.normal);
                VertexPositionInputs fragPosInputs = GetVertexPositionInputs(fragData.position);
                float3 fragWorldN = fragNInputs.normalWS;
                      
                float diffuseFactor = 0;
                float dotLN = dot(light.direction, fragWorldN);

                if(dotLN <= 0.50)
                {
                    diffuseFactor = 0.25;
                }
                // else if(dotLN <= 0.50)
                // {
                //     diffuseFactor = 0.60;
                // }
                else
                {
                    diffuseFactor = 1.0;
                }
                    

                float4 diffuseColor = (_BaseColor * diffuseFactor) * float4(light.color.xyz,1);

                // Get the eye direction view 
                // Fot more -> https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl
                float3 eye = GetCameraPositionWS();
                
                // The eyeDirection is always computed from the fragment to the camera, otherwize, flip it with -1
                float3 eyeDirection = normalize(eye - fragData.positionWS);
                // The edge needs to account the orientation of the normal in world space. Hence the use of GetVertexNormalInputs().normalWS
                // Otherwize, rotating the object will yield unwanted results
                bool edge = dot(eyeDirection, fragWorldN) < _EdgeThreshold;
                float4 edgeColor = (_BaseColor * float4 (0.1,0.1,0.1,1)) * float4(light.color.xyz,1);
                //float4 (eyeDirection.xyz,1)
                // Only edge
                //return edge ? float4 (0,0,0,1) : float4 (1,1,1,1);

                //Simple diffuse
                //return diffuseColor;

                // Edge and stepped diffuse
                return edge ? edgeColor : diffuseColor;
            }
        ENDHLSL
        }
    }
    Fallback Off
}
