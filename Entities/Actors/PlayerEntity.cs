using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAHelpers;
using Microsoft.Xna.Framework.Input;

namespace ReferenceFrames
{
    public class PlayerEntity : ActorEntity
    {
        private const int _energyRegenAmount = 1;
        private const float _normalEnergyRegenLength = 0.5f;
        private const float _justFinishedRegenLength = 0.1f;
        private const int _justFinishedRegenTicks = 4;
        private const int _referenceFrameAttachCost = 3;

        public int CurrentEnergy { get; set; }
        public int MaxEnergy { get; set; }
        public PowerupEntity CurrentPowerup { get; set; }
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
            }
        }

        private int _justFinishedRemainingTicks = 0;
        private Timer _currentRegenTimer;

        private float _floatCurrentOffset = 0f;
        private float _floatChangeAmount = 0.016f;

        public PlayerEntity(Vector2 position, MainGame game) 
            : base(position, game)
        {
            _texture = game.Content.Load<Texture2D>("Player Temp 2");
            MaxEnergy = 10;
            MaxHealth = 16;

            CurrentEnergy = MaxEnergy;
            CurrentHealth = MaxHealth;
            _currentRegenTimer = Timer.Create(_normalEnergyRegenLength, true, (timer) => RegenerateEnergy());
        }

        public override void Update(GameTime elapsedTime)
        {
            ProcessPowerupCollisions();
            ProcessReferenceFrames();
            ProcessEnemyCollisions();

            _isJumping = _input.CurrentKeyboardState.IsKeyDown(Keys.Space);
            int moveDirectionTemp = 1;
            if (_input.IsCurPress(Keys.Left))
            {
                moveDirectionTemp--;
            }
            if (_input.IsCurPress(Keys.Right))
            {
                moveDirectionTemp++;
            }
            _moveDirection = (MoveDirection)moveDirectionTemp;  

            base.Update(elapsedTime);

            if (CurrentPowerup != null)
            {
                CurrentPowerup.Update();
                if (CurrentEnergy <= 0)
                {
                    RemovePowerup();
                }
            }

            //code to make the sprite look like it bobs up and down while floating
            if (IsOnGround)
            {
                if (_floatCurrentOffset >= 1f)
                {
                    _floatChangeAmount = -((float)elapsedTime.ElapsedGameTime.TotalSeconds * 3f);
                }
                if (_floatCurrentOffset <= -1f)
                {
                    _floatChangeAmount = (float)elapsedTime.ElapsedGameTime.TotalSeconds * 3f;
                }
                _floatCurrentOffset += _floatChangeAmount;
            }
        }

        protected override void ProcessMovement()
        {
            if (IsOnGround && !_wasOnGround)
            {
                _floatCurrentOffset = 1f;
            }
            base.ProcessMovement();
            if (SelectedMapEntity != null && SelectedMapEntity is MovingPlatformEntity)
            {
                Vector2 newPos = Position;
                if (!IsOnGround)
                {
                    newPos.Y += SelectedMapEntity.Velocity.Y;
                }
                if (!_isCollidingWithWall)
                {
                    newPos.X += SelectedMapEntity.Velocity.X;
                }
                Position = newPos;
            }
        }

        private void ProcessReferenceFrames()
        {
            if (SelectedMapEntity != null && !_game.Camera.IsInCameraView(SelectedMapEntity.Position))
            {
                SelectedMapEntity.PlayerDetached(this);
                SelectedMapEntity = null;
            }
            if (_input.IsNewPress(Keys.X) && _game.ClosestEntity != null)
            {
                if (SelectedMapEntity == null && CurrentEnergy >= _referenceFrameAttachCost)
                {
                    SelectedMapEntity = _game.ClosestEntity;
                    SelectedMapEntity.PlayerAttached(this);
                    CurrentEnergy -= _referenceFrameAttachCost;
                }
                else if (SelectedMapEntity is PositionAnchorEntity && CurrentEnergy >= _referenceFrameAttachCost)
                {
                    Vector2 positionDelta = _game.ClosestEntity.Position - SelectedMapEntity.Position;
                    Vector2 targetPosition = Position + positionDelta;
                    targetPosition.X = MathFunctions.RoundDownTo(targetPosition.X, 8) / 8;
                    targetPosition.Y = MathFunctions.RoundDownTo(targetPosition.Y, 8) / 8;
                    if (!_level.GetTileIsSolid((int)targetPosition.X, (int)targetPosition.Y) &&
                        targetPosition.Y < _level.LevelSize.Y &&
                        targetPosition.Y > 0)
                    {
                        Position += positionDelta;
                    }
                    SelectedMapEntity.PlayerDetached(this);
                    SelectedMapEntity = null;
                    CurrentEnergy -= _referenceFrameAttachCost;
                }
                else if (SelectedMapEntity is MovingPlatformEntity)
                {
                    SelectedMapEntity.PlayerDetached(this);
                    SelectedMapEntity = null;
                }
            }
        }

        private void ProcessPowerupCollisions()
        {
            int xMinPos = (int)(MathFunctions.RoundDownTo(Position.X - 4, 8) / 8);
            int yMinPos = (int)(MathFunctions.RoundDownTo(Position.Y - 4, 8) / 8);
            int xMaxPos = (int)(MathFunctions.RoundDownTo(Position.X + 4, 8) / 8);
            int yMaxPos = (int)(MathFunctions.RoundDownTo(Position.Y + 4, 8) / 8);

            PowerupEntity upperLeftPowerup = _level.GetTileHasPowerup(xMinPos, yMinPos);
            PowerupEntity upperRightPowerup = _level.GetTileHasPowerup(xMaxPos, yMinPos);
            PowerupEntity lowerLeftPowerup = _level.GetTileHasPowerup(xMinPos, yMaxPos);
            PowerupEntity lowerRightPowerup = _level.GetTileHasPowerup(xMaxPos, yMaxPos);
            if (upperLeftPowerup != null && CurrentEnergy > 0)
            {
                CollectPowerup(upperLeftPowerup);
                return;
            }
            if (upperRightPowerup != null && CurrentEnergy > 0)
            {
                CollectPowerup(upperRightPowerup);
                return;
            }
            if (lowerLeftPowerup != null && CurrentEnergy > 0)
            {
                CollectPowerup(lowerLeftPowerup);
                return;
            }
            if (lowerRightPowerup != null && CurrentEnergy > 0)
            {
                CollectPowerup(lowerRightPowerup);
                return;
            }
        }

        private void ProcessEnemyCollisions()
        {
            foreach (ActorEntity ent in _level.Enemies)
            {
                //needs code to check for major axis and if Y, do something else at least with resolution
                if (MathFunctions.PointIsInArea(this.CollisionPositionTopLeft, ent.CollisionPositionTopLeft, ent.CollisionPositionBotRight) ||
                    MathFunctions.PointIsInArea(this.CollisionPositionBotRight, ent.CollisionPositionTopLeft, ent.CollisionPositionBotRight) ||
                    MathFunctions.PointIsInArea(ent.CollisionPositionTopLeft, this.CollisionPositionTopLeft, this.CollisionPositionBotRight) ||
                    MathFunctions.PointIsInArea(ent.CollisionPositionBotRight, this.CollisionPositionTopLeft, this.CollisionPositionBotRight))
                {
                    if (ent.CollisionsEnabled && CollisionsEnabled)
                    {
                        EnemyEntity enemy = (EnemyEntity)ent;
                        CurrentHealth -= enemy.HealthCostOnCollision;
                        OnCollideWithOtherActor(enemy.Position + enemy.CollisionBoxOffset);
                        enemy.OnCollideWithOtherActor(Position + CollisionBoxOffset);
                    }
                }
            }
        }

        //complete override to implement fake floating
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,
                new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y) + (int)Math.Round(_floatCurrentOffset)),
                null,
                Color.White,
                0f,
                new Vector2(_texture.Width / 2, _texture.Height / 2),
                1f,
                _direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
        }

        protected override void OnLostHealth(int oldHealth, int newHealth)
        {
            if (newHealth <= 0)
            {
                _game.OnPlayerDied();
            }
        }

        private void CollectPowerup(PowerupEntity powerup)
        {
            CurrentPowerup = powerup;
            CurrentPowerup.IsCollected = true;
            if (_currentRegenTimer != null)
            {
                _currentRegenTimer.Stop();
                _currentRegenTimer = null;
            }
        }

        public void RemovePowerup()
        {
            if (CurrentPowerup != null)
            {
                CurrentPowerup.Disable();
                CurrentPowerup = null;
                _currentRegenTimer = Timer.Create(_justFinishedRegenLength, true, (timer) => RegenerateEnergyFast());
                _justFinishedRemainingTicks = _justFinishedRegenTicks;
            }
        }

        private void RegenerateEnergy()
        {
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy += _energyRegenAmount;
            }
        }

        private void RegenerateEnergyFast()
        {
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy += _energyRegenAmount;
            }
            _justFinishedRemainingTicks--;
            if (_justFinishedRemainingTicks <= 0)
            {
                _currentRegenTimer.Stop();
                _currentRegenTimer = null;
                _currentRegenTimer = Timer.Create(_normalEnergyRegenLength, true, (timer) => RegenerateEnergy());
            }
        }
    }
}
