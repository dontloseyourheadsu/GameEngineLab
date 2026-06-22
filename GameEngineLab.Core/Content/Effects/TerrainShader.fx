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
float TileScale;
float4 BaseColor;
float4 DetailColor;

sampler TextureSampler : register(s0);

// Simple pseudo-random hash function
float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

// 2D Value Noise
float Noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    
    // Smoothstep interpolation curves
    float2 u = f * f * (3.0 - 2.0 * f);
    
    return lerp(lerp(Hash(i + float2(0.0, 0.0)), Hash(i + float2(1.0, 0.0)), u.x),
                lerp(Hash(i + float2(0.0, 1.0)), Hash(i + float2(1.0, 1.0)), u.x), u.y);
}

// Fractional Brownian Motion (4 octaves)
float FBM(float2 p)
{
    float value = 0.0;
    float amplitude = 0.5;
    
    for (int i = 0; i < 4; i++)
    {
        value += amplitude * Noise(p);
        p = p * 2.0 + float2(50.0, 50.0);
        amplitude *= 0.5;
    }
    
    return value;
}

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target
{
    // Scale texture coordinates
    float2 uv = texCoord * TileScale;
    
    // Animated slow drift to make the terrain feel alive (e.g. dynamic dust/heat wave)
    uv.x += sin(Time * 0.05) * 0.03;
    uv.y += cos(Time * 0.04) * 0.03;
    
    // Generate noise value
    float n = FBM(uv);
    
    // Double noise modulation for cartoon/Isaac-style rock cracks/ridges
    float cracks = FBM(uv * 3.0 + float2(n * 2.0, n * 2.0));
    float finalNoise = lerp(n, cracks, 0.3);
    
    // Lerp base and detail color
    float4 terrainColor = lerp(BaseColor, DetailColor, finalNoise);
    
    // Combine with draw color tint
    return terrainColor * color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
