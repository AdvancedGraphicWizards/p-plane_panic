Shader "VFX/Character/FeathersNPR"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        //ZWrite Off // Disable depth writing for transparency
        //Blend SrcAlpha OneMinusSrcAlpha // Enable alpha blending
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct VertexInput
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;
                float2 uv      : TEXCOORD0;
            };

            struct VertexOut
            {
                float4 position : SV_POSITION;
                float3 normal   : NORMAL;
                float2 uv       : TEXCOORD0;
            };

            int _ShellIndex; // This is the current shell layer being operated on, it ranges from 0 -> _ShellCount 
			int _ShellCount; // This is the total number of shells, useful for normalizing the shell index
			float _ShellLength; // This is the amount of distance that the shells cover, if this is 1 then the shells will span across 1 world space unit
			float _Density;  // This is the density of the strands, used for initializing the noise
			float _NoiseMin, _NoiseMax; // This is the range of possible hair lengths, which the hash then interpolates between 
			float _Thickness; // This is the thickness of the hair strand
			float _Attenuation; // This is the exponent on the shell height for lighting calculations to fake ambient occlusion (the lack of ambient light)
			float _OcclusionBias; // This is an additive constant on the ambient occlusion in order to make the lighting less harsh and maybe kind of fake in-scattering
			float _ShellDistanceAttenuation; // This is the exponent on determining how far to push the shell outwards, which biases shells downwards or upwards towards the minimum/maximum distance covered
			float _Curvature; // This is the exponent on the physics displacement attenuation, a higher value controls how stiff the hair is
			float _DisplacementStrength; // The strength of the displacement (very complicated)
			
            //* Added a dimension
            float4 _ShellColor; // The color of the shells (very complicated)
			
            float3 _ShellDirection; // The direction the shells are going to point towards, this is updated by the CPU each frame based on user input/movement

            float4 _BaseColor;
            float _Hardness;
            float _PhongExponent;

            float3 _WindDirection;

            //For random value, implementation detail not important
            float hash(uint n) {
				// integer hash copied from Hugo Elias
				n = (n << 13U) ^ n;
				n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
				return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
			}

            VertexOut vert(VertexInput INPUT)
            {
                VertexOut OUTPUT;

                float shellHeight = (float)_ShellIndex / (float)_ShellCount;
                shellHeight = pow(shellHeight, _ShellDistanceAttenuation);

                INPUT.vertex.xyz += INPUT.normal.xyz * _ShellLength * shellHeight;

                float k = pow(shellHeight, _Curvature);
                INPUT.vertex.xyz += _ShellDirection * k * _DisplacementStrength;

                //Fragment input initialization
                OUTPUT.position = TransformObjectToHClip(INPUT.vertex.xyz);
                OUTPUT.normal = normalize(INPUT.normal.xyz);
                OUTPUT.uv = INPUT.uv;

                return OUTPUT;
            }

            float4 frag(VertexOut vert_OUTPUT) : SV_Target
            {
                //Setting the color from Unity C# brings inconsistencies. Build the color by components
                _BaseColor.r = _ShellColor.r;
                _BaseColor.g = _ShellColor.g;
                _BaseColor.b = _ShellColor.b;
                float4 surfaceColor = _BaseColor;


                float2 newUV = vert_OUTPUT.uv * _Density;
                newUV.y *= 8;
                float2 localUV = frac(newUV) * 2 - 1;
                float localDistanceFromCenter = length(localUV);

                uint2 tid = newUV;
				uint seed = tid.x + 100 * tid.y + 100 * 10;

                float shellIndex = _ShellIndex;
                float shellCount = _ShellCount;

                float rand = lerp(_NoiseMin, _NoiseMax, hash(seed));

                float h = shellIndex / shellCount;
                int outsideThickness = (localDistanceFromCenter) > (_Thickness * (rand - h));
                if (outsideThickness && _ShellIndex > 0) discard;

                /*------------------  Physically based illumination  ----------- */


                // Modify the alpha based on layer index to blend layers
                float shellHeight = (float)_ShellIndex / (float)_ShellCount;
                surfaceColor.a *= (1.0 - shellHeight);

                Light light = GetMainLight();
                float4 lightColor = float4(light.color.xyz,1);
                float3 normalWS = GetVertexNormalInputs(vert_OUTPUT.normal).normalWS;
                float lambertCoefficient = max(0, dot(light.direction, normalWS));

                // Toon shading and HalfLambert
                float steppedLambert = 0;
                float dotLN = dot(light.direction, normalWS);
                float halfLambert = dotLN * _Hardness + 1 * _Hardness;

                if(dotLN >= 0.6)
                    steppedLambert = halfLambert;//1.0;
                else if(dotLN >= 0.1)
                    steppedLambert = 0.5;
                else
                    steppedLambert = 0.05;

                float4 diffuseColor = halfLambert * surfaceColor * lightColor;

                //ambient shading
                float ambientLightIntensity = _OcclusionBias;//_AmbientCoefficient;
                float4 ambientColor = surfaceColor * 0.9;
                float4 ambientShading = ambientColor * surfaceColor * ambientLightIntensity;

                float3 cameraPosWS = GetCameraPositionWS();
                // We compute the vector that determines the were the camera is.
                // get the fragment in the viewport in worldspace
                float3 fragmentPosWS = GetVertexPositionInputs(vert_OUTPUT.position.xyz).positionWS;
                float3 eyeDirection = normalize(cameraPosWS - fragmentPosWS);

                // Specular
                float3 lightDirection = normalize(light.direction);
                float3 halfVector = normalize(eyeDirection + lightDirection);
                float phongExponent = _PhongExponent;
                float4 specularK = surfaceColor * 0.8; //specular coefficient, or the specular color of the surface.
                float4 specularIntensity = pow( max(0, dot(halfVector, normalWS) * (lambertCoefficient > 0) ), phongExponent);
                float4 specularColor = specularK * specularIntensity;

                //Fake AmbientOcclusion                
                float ambientOcclusion = pow(h, _Attenuation);
                ambientOcclusion += _OcclusionBias;
                ambientOcclusion = saturate(ambientOcclusion);

                float4 colorBlinnPhongShading =  diffuseColor + specularColor + ambientShading * ambientOcclusion;


                return colorBlinnPhongShading;
            }

            ENDHLSL
        }
    }
    Fallback Off
}