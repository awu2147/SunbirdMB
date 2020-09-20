#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D LightingRender;
sampler2D LightingRenderSampler = sampler_state
{
	Texture = <LightingRender>;
}; 

Texture2D LightingStencilRender;
sampler2D LightingStencilRenderSampler = sampler_state
{
	Texture = <LightingStencilRender>;
};

float4 CurrentLighting;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(LightingRenderSampler, input.TextureCoordinates);
	float4 colorStencil = tex2D(LightingStencilRenderSampler, input.TextureCoordinates);
	
	// Green = Current(Anti)Lighting, Red = Lighting, Black = Deco Stencil (AntiShadow mask).
	// Propagate green to red and black.
    color.g = CurrentLighting.g;
	// Now subtract red channel value from green channel value.
	// The smaller the result the brighter the light (following subtrative blendstate).
    float green = color.g;
    float red = color.r;
    float diff = green - red;
	// Assign result to all channels instead of just green.
    color.rgb = float3(diff, diff, diff);
	// Use second stencil to trim places where light overlaps with background.
    if (colorStencil.g == CurrentLighting.g)
	{ 
        return color.rgba = CurrentLighting.ggga;
    }
	else
	{
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