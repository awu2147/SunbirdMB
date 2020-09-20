#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D WaterRender;
sampler2D WaterRenderSampler = sampler_state
{
    Texture = <WaterRender>;
};

Texture2D WaterStencilRender;
sampler2D WaterStencilRenderSampler = sampler_state
{
    Texture = <WaterStencilRender>;
};

float1 Brightness;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 white = float3(0.2f, 0.2f, 0.2f);
    float4 clear = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float4 color = tex2D(WaterRenderSampler, input.TextureCoordinates);
    float4 colorStencil = tex2D(WaterStencilRenderSampler, input.TextureCoordinates);
    if (colorStencil.r == float1(0))
    {
        return color.a = 0;
    }
    else
    {
        color.r = Brightness - color.r;
        color.b = Brightness - color.b;
        color.g = Brightness - color.g;
        return color;
    }
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};