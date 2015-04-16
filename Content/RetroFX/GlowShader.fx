sampler primaryTex : register(s0);
sampler secondaryTex : register(s1);
uniform extern float2 TextureSize;
uniform extern float BloomThreshhold;
uniform extern float BloomOverlayPercent;
uniform extern float BloomPrimaryPixelPercent;
uniform extern float BloomSecondaryPixelPercent;


float getLuminance(float4 color)
{
	return color.x * 0.2126 + color.y * 0.7152 + color.z * 0.0722;
}

float4 BloomedColor(float2 texCoord: TEXCOORD0) : COLOR
{
    float4 mainPixelColor = tex2D(primaryTex, texCoord) * BloomPrimaryPixelPercent;    
    
    float2 temp = texCoord;
    float2 singlePixelAmount = 1 / TextureSize;
    temp.x += singlePixelAmount.x;
    float4 pixelPlus1X = tex2D(primaryTex, temp);
    temp.x -= singlePixelAmount.x * 2;
    float4 pixelMinus1X = tex2D(primaryTex, temp);
    temp.x += singlePixelAmount.x;
    temp.y += singlePixelAmount.y;
    float4 pixelPlus1Y = tex2D(primaryTex, temp);
    temp.y -= singlePixelAmount.y * 2;
    float4 pixelMinus1Y = tex2D(primaryTex, temp);
    
	mainPixelColor += pixelPlus1X * BloomSecondaryPixelPercent;
    
	mainPixelColor += pixelPlus1Y * BloomSecondaryPixelPercent;
    
	mainPixelColor += pixelMinus1X * BloomSecondaryPixelPercent;
    
	mainPixelColor += pixelMinus1Y * BloomSecondaryPixelPercent;


    return mainPixelColor;
}
float4 BloomCutoff(float2 texCoord : TEXCOORD0) : COLOR
{
    float4 mainPixelColor = tex2D(primaryTex, texCoord);  
    if (getLuminance(mainPixelColor) <= BloomThreshhold)
    {
		mainPixelColor = 0;
    }
    return mainPixelColor;
}
float4 BloomOverlay(float2 texCoord : TEXCOORD0) : COLOR
{
	float4 pixelColor = tex2D(primaryTex, texCoord);
	float4 bloomColor = tex2D(secondaryTex, texCoord);
	pixelColor += bloomColor * BloomOverlayPercent;
	return pixelColor;
}
float4 VignetteOverlay(float2 texCoord : TEXCOORD0) : COLOR
{
	float4 pixelColor = tex2D(primaryTex, texCoord);
	float4 vignetteColor = tex2D(secondaryTex, texCoord);
	//don't multiply alpha here or it will look wierd
	pixelColor.xyz *= lerp(0.8, 1.2, getLuminance(vignetteColor));
	//implement sepia here
	return pixelColor;
}
float4 Resize(float2 texCoord : TEXCOORD0) : COLOR
{
	return tex2D(secondaryTex, texCoord);
}

technique Bloom
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 BloomedColor();
	}
}
technique BloomCutoff
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 BloomCutoff();
	}
}
technique BloomOverlay
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 BloomOverlay();
	}
}
technique VignetteOverlay
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 VignetteOverlay();
	}
}
technique Resize
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 Resize();
	}
}