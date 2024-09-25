Shader "VFX/Environment/FlameBurn"
{
    Properties
    {
        [MainTexture] _BaseMap ("BaseMap", 2D) = "white" {}
        _MaskTex ("MashTex", 2D) = "white" {}
        _GrowingSpeed ("Displacement speed", Float) = 1
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType"="Transparent"
        }
        //LOD 100
        //Blend SrcAlpha One
        Blend SrcAlpha OneMinusSrcAlpha
        //Blend One OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert;
            #pragma fragment frag;

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/writing-shaders-urp-unlit-texture.html
            //TODO study why you need TEXTURE2D, what it does, and why is important
            //Macro for Texture sampling in URP, define in Core.hlsl
            TEXTURE2D(_BaseMap); // Define a texture
            SAMPLER(sampler_BaseMap); // Define the sampler
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            
            //TODO study why the texture has an _ST at the end.
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST; //_ST suffix is necesarry because some Macros use it
                float4 _MaskTex_ST;
                float _GrowingSpeed;
            CBUFFER_END

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
            };

            struct FragmentData
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
            };

            FragmentData vert(MeshData mesh)
            {
                FragmentData fragmentData;

                fragmentData.position = TransformObjectToHClip(mesh.vertex.xyz);

                 //TODO Study TRANSFORM_TEX
                 // This Macro applies offset and tiling transformation 
                 // Looks like this -> #define TRANSFORM_TEX(tex, name) ((tex.xy) * name##_ST.xy + name##_ST.zw)
                fragmentData.uv = TRANSFORM_TEX(mesh.uv, _BaseMap);
                fragmentData.uvMask = TRANSFORM_TEX(mesh.uvMask, _MaskTex);

                return fragmentData;
            };

            float4 frag( FragmentData fragmentData) : SV_Target
            {
                float4 fragmetColor = float4(0,0,0,1);
                float2 centeredUV = fragmentData.uv;//* _Time * 0.1; 
                //centeredUV += _Time * 0.1;

                float4 maskBurnSample = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, fragmentData.uv);
                //fragmetColor = maskBurnSample;
                fragmetColor.a *= maskBurnSample.r;

                return fragmetColor;
            };

            ENDHLSL
        }
    }
}
