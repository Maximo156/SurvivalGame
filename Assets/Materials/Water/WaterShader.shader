Shader "Custom/WaterShader"
{
    Properties
    {
        [Header(Water)]
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        [Header(Waves)]
        _WaveHeight("Wave Height", Float) = 0.0
        _WaveSpeed("Wave Speed", Float) = 0.0
        _WaveFrequency("Wave Frequency", Float) = 0.0

        [Header(Foam)]
        _foamColor("Foam Color", Color) = (1,1,1,1)
        _foamSpeed("Foam Speed", float) = 1.0
        _foamFrequency("Foam Frequency", float) = 1.0
        _foamWidth("Foam Width", float) = 1.0
        _foamCutoff("Foam Cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }
        CGPROGRAM
        #pragma surface surf Standard vertex:vert alpha

        #pragma target 3.0
        #include "Noise.hlsl"

        float _WaveHeight;
        float _WaveSpeed;
        float _WaveFrequency;

        struct Input {
            float3 worldPos;
            float4 screenPos;
            float eyeDepth;
        };
          
        float getOffSet(float2 pos) {
            return _WaveHeight * GetNoise(pos + _Time.y * _WaveSpeed, _WaveFrequency);
        }

        void vert(inout appdata_full v, out Input o) {
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            v.vertex.y += getOffSet(worldPos.xz);

            UNITY_INITIALIZE_OUTPUT(Input, o);
            COMPUTE_EYEDEPTH(o.eyeDepth);
        }

        fixed4 _Color;
        float _Glossiness;
        sampler2D _CameraDepthTexture;

        fixed4 _foamColor;
        float _foamSpeed;
        float _foamFrequency;
        float _foamWidth;
        float _foamCutoff;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            

            float2 screenSpaceUV = IN.screenPos.xy / IN.screenPos.w;
            float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            float sceneZ = LinearEyeDepth(rawZ);
            float partZ = IN.eyeDepth;
            float depth = sceneZ - partZ;

            float foamStrength = (1 - saturate(depth / _foamWidth)) * (GetNoise(IN.worldPos.xz + _Time.y * _foamSpeed, _foamFrequency) + 0.5);// / _foamFrequency + _Time.y * _foamSpeed);

            foamStrength = step(_foamCutoff, foamStrength);

            // Albedo comes from a texture tinted by color
            fixed4 c =  _Color;
            o.Albedo = (foamStrength * _foamColor) + (1 - foamStrength) * c.rgb;

            // Metallic and smoothness come from slider variables
            o.Smoothness = (1 - foamStrength) * _Glossiness;
            o.Alpha = (foamStrength * _foamColor.a + (1 - foamStrength) * c.a);

            float3 ddxPos = ddx(IN.worldPos);
            float3 ddyPos = ddy(IN.worldPos) * _ProjectionParams.x;
            float3 normal = normalize(cross(ddxPos, ddyPos));
            o.Normal = normal;
        }
        ENDCG
    }     
}
