using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework;

namespace ReferenceFrames
{
    class PlayerUI
    {
        private const string healthBarPrefix = "Health Bar ";
        private const string energyBarPrefix = "Energy Bar ";
        private const string uiDirectory = "UI";
        private const int _powerupPadding = 2;

        private const int _barsBaseOffsetX = 7;
        private const int _healthBarOffsetY = 1;
        private const int _energyBarOffsetY = 7;
        private const int _healthBarSegmentSize = 1;
        private const int _energyBarSegmentSize = 1;
        private static Vector2 _barFillOffset = new Vector2(1);

        private SpriteBatch _batch;
        private PlayerEntity _player;
        private Texture2D _uiIcons;
        private Texture2D _powerupBackground;

        private Dictionary<int, Texture2D> _healthBarTextures;
        private Dictionary<int, Texture2D> _energyBarTextures;

        public PlayerUI(MainGame game, SpriteBatch batch, PlayerEntity player)
        {
            _batch = batch;
            _player = player;
            _healthBarTextures = new Dictionary<int, Texture2D>();
            _energyBarTextures = new Dictionary<int, Texture2D>();
            _uiIcons = game.Content.Load<Texture2D>(Path.Combine(uiDirectory, "Health + Energy Icons"));
            _powerupBackground = game.Content.Load<Texture2D>(Path.Combine(uiDirectory, "Powerup Background"));

            _healthBarTextures.Add((int)UITextureType.BarSegment, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, healthBarPrefix + "Segment")));
            _healthBarTextures.Add((int)UITextureType.BarSegmentLeft, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, healthBarPrefix + "Segment Left")));
            _healthBarTextures.Add((int)UITextureType.BarSegmentRight, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, healthBarPrefix + "Segment Right")));
            _healthBarTextures.Add((int)UITextureType.BarSegmentSingle, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, healthBarPrefix + "Segment Single")));
            _healthBarTextures.Add((int)UITextureType.Tick, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, healthBarPrefix + "Tick")));
            _healthBarTextures.Add((int)UITextureType.TickFull, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, healthBarPrefix + "Tick Full")));

            _energyBarTextures.Add((int)UITextureType.BarSegment, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, energyBarPrefix + "Segment")));
            _energyBarTextures.Add((int)UITextureType.BarSegmentLeft, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, energyBarPrefix + "Segment Left")));
            _energyBarTextures.Add((int)UITextureType.BarSegmentRight, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, energyBarPrefix + "Segment Right")));
            _energyBarTextures.Add((int)UITextureType.BarSegmentSingle, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, energyBarPrefix + "Segment Single")));
            _energyBarTextures.Add((int)UITextureType.Tick, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, energyBarPrefix + "Tick")));
            _energyBarTextures.Add((int)UITextureType.TickFull, game.Content.Load<Texture2D>(Path.Combine(uiDirectory, energyBarPrefix + "Tick Full")));

        }

        public void Draw()
        {
            _batch.Draw(_uiIcons, new Vector2(1), Color.White);
            _batch.Draw(_powerupBackground, (MainGame.ScreenSize / 4) - (new Vector2(_powerupBackground.Width, _powerupBackground.Height)), Color.White);

            if (_player.CurrentPowerup != null)
            {
                _batch.Draw(
                    _player.CurrentPowerup.UITexture,
                    (MainGame.ScreenSize / 4) - new Vector2(_powerupBackground.Width - _powerupPadding, _powerupBackground.Height - _powerupPadding), 
                    Color.White);
            }

            DrawBarBackground(_player.MaxHealth, _healthBarSegmentSize, _healthBarOffsetY, _healthBarTextures);
            DrawBarForeground(_player.CurrentHealth, _healthBarOffsetY, _healthBarTextures);
            DrawBarBackground(_player.MaxEnergy, _energyBarSegmentSize, _energyBarOffsetY, _energyBarTextures);
            DrawBarForeground(_player.CurrentEnergy, _energyBarOffsetY, _energyBarTextures);
        }

        private void DrawBarBackground(int segmentCount, int segmentSize, int yOffset, Dictionary<int, Texture2D> textures)
        {
            if (segmentCount > 1)
            {
                int xOffset = _barsBaseOffsetX;
                _batch.Draw(textures[(int)UITextureType.BarSegmentLeft], new Vector2(xOffset, yOffset), Color.White);
                xOffset += segmentSize + 1; //+1 is because there is an extra pixel on barsegmentleft texture
                if (segmentCount > 2)
                {
                    for (int x = 1; x < segmentCount - 1; x++)
                    {
                        _batch.Draw(textures[(int)UITextureType.BarSegment], new Vector2(xOffset, yOffset), Color.White);
                        xOffset += segmentSize;
                    }
                }
                _batch.Draw(textures[(int)UITextureType.BarSegmentRight], new Vector2(xOffset, yOffset), Color.White);

            }
            else
            {
                _batch.Draw(textures[(int)UITextureType.BarSegmentSingle], new Vector2(_barsBaseOffsetX, yOffset), Color.White);
            }
        }

        private void DrawBarForeground(int fillAmount, int offsetY, Dictionary<int, Texture2D> textures)
        {
            if (fillAmount > 1)
            {
                Vector2 position = new Vector2(_barsBaseOffsetX, offsetY) + _barFillOffset;
                for (int i = 1; i < fillAmount; i++)
                {
                    _batch.Draw(textures[(int)UITextureType.Tick], position, Color.White);
                    position.X += 1;
                }
                _batch.Draw(textures[(int)UITextureType.TickFull], position, Color.White);
            }
            else if (fillAmount > 0)
            {
                Vector2 position = new Vector2(_barsBaseOffsetX, offsetY) + _barFillOffset;
                _batch.Draw(textures[(int)UITextureType.TickFull], position, Color.White);
            }
        }

        private enum UITextureType
        {
            BarSegment = 0,
            BarSegmentLeft = 1,
            BarSegmentRight = 2,
            BarSegmentSingle = 3,
            Tick = 4,
            TickFull = 5,
        }
    }
}
