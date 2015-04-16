using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using XNAHelpers;

namespace ReferenceFrames
{
    class RetroGfxRenderer
    {
        private SpriteBatch _spriteBatch;
        private GraphicsDevice _device;
        private ContentManager _content;
        private Vector2 _pixelSize;
        private int _bloomIterations;
        private float _bloomOverlayPercent;
        private float _bloomThreshhold;
        private float _bloomPrimaryPixelPercent;
        private float _bloomSecondaryPixelPercent;

        private Texture2D _pixelTex;
        private Texture2D _vignetteTex;
        RenderTarget2D _bloomRenderTarget1;
        RenderTarget2D _bloomRenderTarget2;
        RenderTarget2D _bloomCompositeRenderTarget;
        RenderTarget2D _bloomCutoffRenderTarget;
        RenderTarget2D _vignetteRenderTarget;
        RenderTarget2D _sceneRenderTarget;
        public TextureRenderingMode TextureMode { get; set; }
        public bool SkipPostProcessing { get; set; }

        public float BloomIterations
        {
            get
            {
                return _bloomIterations;
            }
            set
            {
                _bloomIterations = (int)MathHelper.Clamp(value, 1, 15);
            }
        }

        public float BloomThreshhold
        {
            get
            {
                return _bloomThreshhold;
            }
            set
            {
                _bloomEffect.Parameters["BloomThreshhold"].SetValue(MathHelper.Clamp(value, 0, 1));
                _bloomThreshhold = MathHelper.Clamp(value, 0, 1);
            }

        }
        public float BloomOverlayPercent
        {
            get
            {
                return _bloomOverlayPercent;
            }
            set
            {
                _bloomEffect.Parameters["BloomOverlayPercent"].SetValue(MathHelper.Clamp(value, 0, 1));
                _bloomOverlayPercent = MathHelper.Clamp(value, 0, 1);
            }

        }
        public float BloomPrimaryPixelPercent
        {
            get
            {
                return _bloomPrimaryPixelPercent;
            }
            set
            {
                _bloomEffect.Parameters["BloomPrimaryPixelPercent"].SetValue(MathHelper.Clamp(value, 0.1f, 0.5f));
                _bloomPrimaryPixelPercent = MathHelper.Clamp(value, 0.1f, 0.5f);
            }
        }
        public float BloomSecondaryPixelPercent
        {
            get
            {
                return _bloomSecondaryPixelPercent;
            }
            set
            {
                _bloomEffect.Parameters["BloomSecondaryPixelPercent"].SetValue(MathHelper.Clamp(value, 0.1f, 0.5f));
                _bloomSecondaryPixelPercent = MathHelper.Clamp(value, 0.1f, 0.5f);
            }
        }

        Effect _bloomEffect;

        public RetroGfxRenderer(
            GraphicsDevice device, 
            SpriteBatch spriteBatch,
            ContentManager content,
            string glowShaderName, 
            string pixelTextureName, 
            string scanlinesTextureName, 
            string vignetteOverlayName)
        {
            _device = device;
            _spriteBatch = spriteBatch;
            _content = content;
            _pixelTex = _content.Load<Texture2D>(pixelTextureName);
            if (_pixelTex.Width != _pixelTex.Height)
            {
                throw new ArgumentException("The pixel texture must be square.");
            }
            _pixelSize = new Vector2(_pixelTex.Width, _pixelTex.Height);
            Texture2D scanlinesTex = _content.Load<Texture2D>(scanlinesTextureName);
            if (vignetteOverlayName != null)
            {
                _vignetteTex = _content.Load<Texture2D>(vignetteOverlayName);
            }
            _bloomEffect = _content.Load<Effect>(glowShaderName);
            BloomThreshhold = 0.7f;
            BloomOverlayPercent = 0.5f;
            BloomIterations = 15;
            BloomPrimaryPixelPercent = 0.25f;
            BloomSecondaryPixelPercent = 0.2f;
            TextureMode = TextureRenderingMode.Both;

            RenderTarget2D pixelCompositeTarget = new RenderTarget2D(
                _device,
                (int)_pixelSize.X,
                (int)_pixelSize.Y,
                1,
                _device.PresentationParameters.BackBufferFormat);

            //composite pixel texture for faster rendering
            RenderTarget2D backBuffer = (RenderTarget2D)_device.GetRenderTarget(0);
            _device.SetRenderTarget(0, pixelCompositeTarget);
            _device.Clear(Color.Black);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_pixelTex, Vector2.Zero, Color.White);
            spriteBatch.Draw(scanlinesTex, Vector2.Zero, new Color(Color.White, 128));
            spriteBatch.End();
            _device.SetRenderTarget(0, backBuffer);
            _pixelTex = pixelCompositeTarget.GetTexture();
        }

        public Texture2D RenderEffect(Texture2D baseTexture, RenderTarget2D backBuffer)
        {
            Vector2 finishedTextureSize = new Vector2(baseTexture.Width * _pixelSize.X, baseTexture.Height * _pixelSize.Y);
            _sceneRenderTarget = new RenderTarget2D(
                _device,
                (int)finishedTextureSize.X,
                (int)finishedTextureSize.Y,
                1,
                _device.PresentationParameters.BackBufferFormat);
            if (SkipPostProcessing)
            {
                return RenderEffectNoPostProcessing(baseTexture, finishedTextureSize, backBuffer);
            }
            _bloomCompositeRenderTarget = new RenderTarget2D(
                _device,
                (int)baseTexture.Width,
                (int)baseTexture.Height,
                1,
                _device.PresentationParameters.BackBufferFormat);
            _bloomCutoffRenderTarget = new RenderTarget2D(
                _device,
                (int)baseTexture.Width,
                (int)baseTexture.Height,
                1,
                _device.PresentationParameters.BackBufferFormat);
            _vignetteRenderTarget = new RenderTarget2D(
                _device,
                (int)baseTexture.Width,
                (int)baseTexture.Height,
                1,
                _device.PresentationParameters.BackBufferFormat);
            _bloomRenderTarget1 = new RenderTarget2D(
                _device,
                (int)baseTexture.Width,
                (int)baseTexture.Height,
                1,
                _device.PresentationParameters.BackBufferFormat);
            _bloomRenderTarget2 = new RenderTarget2D(
                _device,
                (int)baseTexture.Width,
                (int)baseTexture.Height,
                1,
                _device.PresentationParameters.BackBufferFormat);

            _bloomEffect.Parameters["TextureSize"].SetValue(new Vector2(baseTexture.Width, baseTexture.Height));

            DepthStencilBuffer baseDepthStencilBuffer = _device.DepthStencilBuffer;
            _device.DepthStencilBuffer = null;
            Texture2D vignettedBase = baseTexture;
            if (_vignetteTex != null)
            {
                _bloomEffect.CurrentTechnique = _bloomEffect.Techniques["VignetteOverlay"];
                _device.Textures[1] = _vignetteTex;
                _device.SetRenderTarget(0, _vignetteRenderTarget);
                _device.Clear(Color.Black);
                _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                _bloomEffect.Begin();
                _bloomEffect.CurrentTechnique.Passes[0].Begin();
                _spriteBatch.Draw(baseTexture, Vector2.Zero, Color.White);
                _spriteBatch.End();
                _bloomEffect.CurrentTechnique.Passes[0].End();
                _bloomEffect.End();
                _device.SetRenderTarget(0, backBuffer);
                vignettedBase = _vignetteRenderTarget.GetTexture();
            }

            _bloomEffect.CurrentTechnique = _bloomEffect.Techniques["BloomCutoff"];
            _device.SetRenderTarget(0, _bloomCutoffRenderTarget);
            _device.Clear(Color.Black);
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            _bloomEffect.Begin();
            _bloomEffect.CurrentTechnique.Passes[0].Begin();
            _spriteBatch.Draw(vignettedBase, Vector2.Zero, Color.White);
            _spriteBatch.End();
            _bloomEffect.CurrentTechnique.Passes[0].End();
            _bloomEffect.End();
            _device.SetRenderTarget(0, backBuffer);
            Texture2D bloomTexture = _bloomCutoffRenderTarget.GetTexture();
            _bloomEffect.CurrentTechnique = _bloomEffect.Techniques["Bloom"];

            for (int i = 0; i < _bloomIterations; i++)
            {
                //alternate between render targets
                RenderTarget2D currentRenderTarget = i % 2 == 1 ? _bloomRenderTarget2 : _bloomRenderTarget1;
                RenderTarget2D otherRenderTarget = i % 2 == 1 ? _bloomRenderTarget1 : _bloomRenderTarget2;

                _device.SetRenderTarget(0, currentRenderTarget);
                _device.Clear(Color.Black);
                _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                _bloomEffect.Begin();
                _bloomEffect.CurrentTechnique.Passes[0].Begin();
                _spriteBatch.Draw(bloomTexture, Vector2.Zero, Color.White);
                _spriteBatch.End();
                _bloomEffect.CurrentTechnique.Passes[0].End();
                _bloomEffect.End();

                _device.SetRenderTarget(0, otherRenderTarget);
                bloomTexture = currentRenderTarget.GetTexture();
            }
            _device.SetRenderTarget(0, _bloomCompositeRenderTarget);
            _device.Clear(Color.Black);
            if (TextureMode == TextureRenderingMode.Both)
            {
                _bloomEffect.CurrentTechnique = _bloomEffect.Techniques["BloomOverlay"];
                _device.Textures[1] = bloomTexture;

                _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                _bloomEffect.Begin();
                _bloomEffect.CurrentTechnique.Passes[0].Begin();
                _spriteBatch.Draw(vignettedBase, Vector2.Zero, Color.White);
                _spriteBatch.End();
                _bloomEffect.CurrentTechnique.Passes[0].End();
                _bloomEffect.End();
            }
            else if (TextureMode == TextureRenderingMode.BaseOnly)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(baseTexture, Vector2.Zero, Color.White);
                _spriteBatch.End();
            }
            else if (TextureMode == TextureRenderingMode.BloomOnly)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(bloomTexture, Vector2.Zero, new Color(BloomOverlayPercent, BloomOverlayPercent, BloomOverlayPercent, 1));
                _spriteBatch.End();
            }
            _device.SetRenderTarget(0, backBuffer);
            _device.Clear(Color.Black);

            Texture2D bloomedBaseTexture = _bloomCompositeRenderTarget.GetTexture();


            Color[] imageColors = new Color[baseTexture.Width * baseTexture.Height];
            bloomedBaseTexture.GetData<Color>(imageColors);

            _device.SetRenderTarget(0, _sceneRenderTarget);
            _spriteBatch.Begin();
            for (int x = 0; x < (int)baseTexture.Width; x++)
            {
                for (int y = 0; y < (int)baseTexture.Height; y++)
                {
                    Vector2 location = new Vector2(x * _pixelSize.X, y * _pixelSize.Y);
                    _spriteBatch.Draw(_pixelTex, location, imageColors[x + (baseTexture.Width * y)]);
                }
            }
            _spriteBatch.End();
            _device.SetRenderTarget(0, backBuffer);

            _device.DepthStencilBuffer = baseDepthStencilBuffer;
            return _sceneRenderTarget.GetTexture();
        }

        private Texture2D RenderEffectNoPostProcessing(Texture2D baseTexture, Vector2 finishedTextureSize, RenderTarget2D backBuffer)
        {
            Texture2D tex = new Texture2D(_device, (int)finishedTextureSize.X, (int)finishedTextureSize.Y);
            Color[] texColors = new Color[(int)finishedTextureSize.X * (int)finishedTextureSize.Y];
            for (int x = 0; x < finishedTextureSize.X; x++)
            {
                for (int y = 0; y < finishedTextureSize.Y; y++)
                {
                    texColors[x + (y * x)] = Color.White;
                }
            }
            _sceneRenderTarget = new RenderTarget2D(
                _device,
                (int)finishedTextureSize.X,
                (int)finishedTextureSize.Y,
                1,
                _device.PresentationParameters.BackBufferFormat);
            _device.SetRenderTarget(0, _sceneRenderTarget);
            _device.Textures[1] = baseTexture;
            _bloomEffect.CurrentTechnique = _bloomEffect.Techniques["Resize"];

            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            _bloomEffect.Begin();
            _bloomEffect.CurrentTechnique.Passes[0].Begin();
            _spriteBatch.Draw(tex, Vector2.Zero, Color.White);
            _spriteBatch.End();
            _bloomEffect.CurrentTechnique.Passes[0].End();
            _bloomEffect.End();

            _device.SetRenderTarget(0, backBuffer);
            return _sceneRenderTarget.GetTexture();
        }
    }

    enum TextureRenderingMode
    {
        Both = 0,
        BloomOnly = 1,
        BaseOnly = 2,
    }
}
