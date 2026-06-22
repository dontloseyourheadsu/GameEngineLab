#if OPENGL
    #define SV_Target COLOR0
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Parameters passed from game code
float Time;
float Intensity;
float OuterRadius;
float InnerRadius;
float4 LightColor;

sampler TextureSampler : register(s0);

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target
{
    // Calculate distance from center (0.5, 0.5) scaled from 0.0 to 1.0
    float2 center = float2(0.5, 0.5);
    float dist = distance(texCoord, center) * 2.0;
    
    // Smooth breathing animation using sine wave
    float breath = sin(Time * 2.5) * 0.04;
    float outer = OuterRadius + breath;
    float inner = InnerRadius;
    
    float glow = 0.0;
    if (dist < inner)
    {
        glow = 1.0;
    }
    else if (dist < outer)
    {
        // Smooth transition and quadratic falloff for soft realistic glow
        float lerpFactor = (dist - inner) / (outer - inner);
        glow = 1.0 - lerpFactor;
        glow = glow * glow; // Exponential decay
    }
    
    // Tint with light color and intensity
    float4 finalColor = LightColor * glow * Intensity;
    
    return finalColor * color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
