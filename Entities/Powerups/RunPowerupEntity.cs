using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Input;
using XNAHelpers;

namespace ReferenceFrames
{
    public class RunPowerupEntity : PowerupEntity
    {
        private const float _teleportLengthPerEnergyUnit = 4;
        private const float _maxTeleportLength = 48;

        public RunPowerupEntity(Vector2 position, MainGame game)
            : base(position, game)
        {
            _powerupSprite = game.Content.Load<Texture2D>(Path.Combine("Powerups", Path.Combine("Run", "Level")));
            UITexture = game.Content.Load<Texture2D>(Path.Combine("Powerups", Path.Combine("Run", "UI")));
            _energyDrainTickLength = 0.5f;
            _energyDrainAmount = 1;
        }
        public override void Update()
        {
            if (_input.IsCurPress(Keys.Z))
            {
                Player.IsRunning = true;
                EnergyDraining = true;
            }
            else
            {
                Player.IsRunning = false;
                EnergyDraining = false;
            }
        }

        public override void Disable()
        {
            Player.IsRunning = false;
            base.Disable();
        }
    }
}
