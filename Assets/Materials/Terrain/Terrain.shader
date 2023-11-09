Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _SandColor("Sand", Color) = (1,1,1,1)
        _SandStop("SandStop", Range(0,1)) = 0.0

        _GrassColor("Grass", Color) = (1,1,1,1)
        _GrassStop("GrassStop", Range(0,1)) = 0.0

        _RockColor("Rock", Color) = (1,1,1,1)
        _RockStop("RockStop", Range(0,1)) = 0.0

        _SnowColor("Snow", Color) = (1,1,1,1)

        _BlendWidth("BlendWidth", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
        };

        fixed4 _SandColor;
        float _SandStop;

        fixed4 _GrassColor;
        float _GrassStop;

        fixed4 _RockColor;
        float _RockStop;

        fixed4 _SnowColor;
        
        float _BlendWidth;

        float MaxHeight;

        float invLerp(float from, float to, float value) {
            return clamp((value - from) / (to - from), 0, 1);
        }

        fixed4 Blend(fixed4 a, fixed4 b, float cutoff, float height) 
        {
            float invL = invLerp(cutoff - _BlendWidth, cutoff, height);
            return b * invL + (1 - invL) * a;
        }

        fixed4 GetColorByHeight(float height) 
        {
            fixed4 res = Blend(_SandColor, _GrassColor, _SandStop * MaxHeight, height);
            res = Blend(res, _RockColor, _GrassStop * MaxHeight, height);
            res = Blend(res, _SnowColor, _RockStop * MaxHeight, height);
            return res;
        }


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = GetColorByHeight(IN.worldPos.y);
            o.Albedo = c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
