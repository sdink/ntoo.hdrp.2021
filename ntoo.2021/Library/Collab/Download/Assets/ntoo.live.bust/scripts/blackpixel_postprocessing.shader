Shader "Hidden/Shader/blackpixel_postprocessing"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    TEXTURE2D_X(_InputTexture);

    float random (float2 st) {
        return frac(sin(dot(st.xy,
                            float2(12.9898,78.233)))*
            43758.5453123);
    }

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float _BlackPixelAmount = 0.75
        float _PixelateAmount = 0.50
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float2 uv = input.texcoord;
        float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
        float2 pixelation = _ScreenSize.xy*_PixelateAmount;
        uv = (floor(uv * pixelation)) / pixelation;
        
        float getRandomValue = _BlackPixelAmount * random(outColor.xy+uv) * (1.0-((outColor.x+outColor.y+outColor.z)/3.));
        float getRandomValue2 = _BlackPixelAmount * random(outColor.xy) * (1.0-((outColor.x+outColor.y+outColor.z)/3.));
        float  oneOrZero= 1.0 - floor(getRandomValue+0.5);
        
        float morePixelation = (1.0 - getRandomValue2);
        outColor.rgb = col.rgb*oneOrZero * morePixelation;
        return float4(outColor, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "blackpixel_postprocessing"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
