using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNAHelpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ReferenceFrames
{
    public abstract class PowerupEntity
    {
        protected static int _energyDrainAmount = 1;
        protected static float _energyDrainTickLength = 1;

        public bool EnergyDraining
        {
            get
            {
                return _energyDraining;
            }
            set
            {
                if (!_energyDraining && value)
                {
                    _energyDrainTimer = Timer.Create(_energyDrainTickLength, false, (timer) => DrainEnergy());
                    _energyDraining = value;
                    DrainEnergy();
                }
                else if (_energyDraining && !value)
                {
                    _energyDrainTimer.Stop();
                    _energyDrainTimer = null;
                }
                _energyDraining = value;
            }
        }
        public bool IsCollected { get; set; }
        public Texture2D UITexture { get; set; }
        public PlayerEntity Player { get; set; }
        public Vector2 Position { get; set; }

        protected bool _energyDraining = false;
        protected Timer _energyDrainTimer;
        protected InputHelper _input;
        protected Texture2D _powerupSprite;

        public PowerupEntity(Vector2 position, MainGame game)
        {
            Position = position;
            _input = game.Input;
        }

        public void Draw(SpriteBatch batch)
        {
            if (!IsCollected)
            {
                batch.Draw(_powerupSprite, Position * 8, Color.White);
            }
        }

        public virtual void Disable()
        {
            if (_energyDrainTimer != null)
            {
                _energyDrainTimer.Stop();
                _energyDrainTimer = null;
            }
            _energyDraining = false;
            IsCollected = false;
        }

        public abstract void Update();

        protected void DrainEnergy()
        {
            if (_energyDraining)
            {
                Player.CurrentEnergy -= _energyDrainAmount;
                if (_energyDraining)
                {
                    _energyDrainTimer.Stop();
                    _energyDrainTimer = null;
                    _energyDrainTimer = Timer.Create(_energyDrainTickLength, false, (timer) => DrainEnergy());
                }
            }
        }

        protected void DrainEnergySingle()
        {
            Player.CurrentEnergy -= _energyDrainAmount;
        }

        protected void DrainAllEnergy()
        {
            Player.CurrentEnergy = 0;
        }

    }
}
