using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace ReferenceFrames
{
    class SwordPowerupEntity : PowerupEntity
    {
        public SwordPowerupEntity(Vector2 position, MainGame game)
            : base(position, game)
        {
            _powerupSprite = game.Content.Load<Texture2D>(Path.Combine("Powerups", Path.Combine("Sword", "Level")));
            UITexture = game.Content.Load<Texture2D>(Path.Combine("Powerups", Path.Combine("Sword", "UI")));
            _energyDrainTickLength = 1f;
            _energyDrainAmount = 2;
        }

        public override void Update()
        {
            //implement soon
        }
    }
}
