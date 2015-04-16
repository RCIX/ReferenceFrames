using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAHelpers;
using System.Diagnostics;

namespace ReferenceFrames
{
    public class MapEntity
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsClosestReferenceFrame {
            get
            {
                return _isClosestReferenceFrame;
            }
            set
            {
                _isClosestReferenceFrame = value;
            }
        }
        public bool IsSelectedReferenceFrame { get; set; }

        private bool _isClosestReferenceFrame = false;

        protected PlayerEntity _player;
        protected Texture2D _texture;
        protected MainGame _game;

        public MapEntity(Vector2 position, MainGame game)
        {
            Position = position;
            _game = game;
        }
        public virtual void PlayerAttached(PlayerEntity player)
        {
            _player = player;
            IsSelectedReferenceFrame = true;
        }
        public virtual void PlayerDetached(PlayerEntity player)
        {
            _player = null;
            IsSelectedReferenceFrame = false;
        }
        public virtual void Update(GameTime elapsedTime)
        {

        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture, 
                new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y)), 
                new Rectangle(0, 0, 8, 8), 
                Color.White, 
                0, 
                new Vector2(4), 
                1, 
                SpriteEffects.None,
                0);
            if (this.IsClosestReferenceFrame)
            {
                spriteBatch.Draw(
                    _texture,
                    new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y)),
                    new Rectangle(8, 0, 8, 8),
                    Color.White,
                    0,
                    new Vector2(4),
                    1,
                    SpriteEffects.None,
                    0);
            }
            if (this.IsSelectedReferenceFrame)
            {
                spriteBatch.Draw(
                    _texture,
                    new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y)),
                    new Rectangle(16, 0, 8, 8),
                    Color.White,
                    0,
                    new Vector2(4),
                    1,
                    SpriteEffects.None,
                    0);
            }
        }
    }
}
