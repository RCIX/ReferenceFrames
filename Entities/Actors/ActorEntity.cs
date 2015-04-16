using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAHelpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace ReferenceFrames
{
    public class ActorEntity
    {
        public ActorMotionProperties MotionProperties { get; set; }

        public bool IsOnGround { get; set; }
        public bool IsResetting { get; set; }
        public bool ControlsEnabled { get; set; }
        public bool CollisionsEnabled { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 CollisionBoxSize { get; set; }
        public Vector2 CollisionBoxOffset { get; set; }
        public Vector2 CollisionPositionTopLeft
        {
            get
            {
                return (Position + CollisionBoxOffset) - (CollisionBoxSize / 2);
            }
        }
        public Vector2 CollisionPositionBotRight 
        {
            get
            {
                return (Position + CollisionBoxOffset) + (CollisionBoxSize / 2);
            }
        }

        public MapEntity SelectedMapEntity { get; set; }

        public int CurrentHealth 
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                if (value < _currentHealth)
                {
                    OnLostHealth(_currentHealth, value);
                }
                _currentHealth = Math.Max(value, 0);
            }
        }
        public int MaxHealth { get; set; }


        protected int _currentHealth;
        protected bool _isRunning;
        protected bool _isJumping;
        protected bool _wasJumping;
        protected MoveDirection _moveDirection;

        protected Texture2D _texture;
        protected InputHelper _input;
        protected MainGame _game;
        protected Level _level;

        protected bool _wasOnGround;
        protected bool _hasReleasedJump;
        protected bool _direction = false;
        protected bool _isCollidingWithWall = false;

        protected float _currentJumpFloatAmount;


        public ActorEntity(Vector2 position, MainGame game)
        {
            _input = game.Input;
            _level = game.CurrentLevel;
            _game = game;

            
            MaxHealth = 16;
            CollisionBoxSize = new Vector2(8);
            CollisionBoxOffset = Vector2.Zero;
            ControlsEnabled = true;
            CollisionsEnabled = true;
            CurrentHealth = MaxHealth;
            Position = position;

            MotionProperties = new ActorMotionProperties();
        }

        //update code

        public virtual void Update(GameTime elapsedTime)
        {
            ProcessMovement();
            ProcessCollisions();

            //calculate direction of sprite
            bool newDirection = Velocity.X < 0;
            if (newDirection != _direction && Math.Abs(Velocity.X) > MotionProperties.DirectionFlipMinVelocity)
            {
                _direction = newDirection;
            }

        }

        protected virtual void ProcessMovement()
        {
            if (_input.IsNewPress(Keys.K))
            {

                CollisionBoxSize = new Vector2(CollisionBoxSize.X - 1, CollisionBoxSize.Y - 1);
            }
            if (_input.IsNewPress(Keys.L))
            {
                CollisionBoxSize = new Vector2(CollisionBoxSize.X + 1, CollisionBoxSize.Y + 1);
            }
            Vector2 inputVelocity = Vector2.Zero;
            float maxMovementRate = _isRunning ? MotionProperties.MaxRunSpeed : MotionProperties.MaxWalkSpeed;
            float accelerationRate = _isRunning ? MotionProperties.RunAcceleration : MotionProperties.WalkAcceleration;

            if (ControlsEnabled)
            {
                if (_moveDirection == MoveDirection.Left && Velocity.X > -maxMovementRate)
                {
                    inputVelocity.X -= accelerationRate;
                }
                if (_moveDirection == MoveDirection.Right && Velocity.X < maxMovementRate)
                {
                    inputVelocity.X += accelerationRate;
                }

                if ((_isJumping && !_wasJumping) && (IsOnGround && _wasOnGround))
                {
                    inputVelocity.Y -= _isRunning ? MotionProperties.RunJumpInitialAmount : MotionProperties.WalkJumpInitialAmount;
                    _currentJumpFloatAmount = _isRunning ? MotionProperties.RunJumpFloatAmount : MotionProperties.WalkJumpFloatAmount;
                    _hasReleasedJump = false;
                }
                if ((_isJumping && _wasJumping) && !_hasReleasedJump)
                {
                    inputVelocity.Y -= _currentJumpFloatAmount;
                    _currentJumpFloatAmount *= MotionProperties.JumpFloatDecayAmount;
                }
                if ((!_isJumping && _wasJumping) || Velocity.Y + inputVelocity.Y > 0.05f)
                {
                    _hasReleasedJump = true;
                    _currentJumpFloatAmount = 0;
                }
            }

            if (ControlsEnabled)
            {
                inputVelocity.Y += MotionProperties.FallAcceleration;
            }
            else
            {
                inputVelocity.Y += MotionProperties.CollisionFallAcceleration;
            }
            if (!ControlsEnabled && (!_wasOnGround && IsOnGround))
            {
                ControlsEnabled = true;
            }
            if (!CollisionsEnabled && (!_wasOnGround && IsOnGround))
            {
                CollisionsEnabled = true;
            }
            if (inputVelocity.X == 0)
            {
                if (ControlsEnabled)
                {
                    inputVelocity.X = -(Velocity.X * MotionProperties.HorizontalMovementDamping);
                }
                else
                {
                    inputVelocity.X = -(Velocity.X * MotionProperties.CollisionHorizontalMovementDamping);
                }
            }
            Velocity += inputVelocity;
            if (Velocity != Vector2.Zero)
            {
                Vector2 velTemp = Velocity;
                velTemp *= 8;
                _game.DrawDebugLine(Position, Position + velTemp, Color.White);
            }

            Position += Velocity;

            _wasJumping = _isJumping;
        }

        protected virtual void ProcessCollisions()
        {
            //get list of all intersecting grid tiles
            //for each tile
            //calculate interpenetration and minimal repulsion vector
            //sum vectors
            //move actor by largest axis

            _wasOnGround = IsOnGround;

            IsOnGround = false;
            bool shouldKeepResolving = true;
            Vector2 collisionPosition = Position + CollisionBoxOffset;

            int gridStartIndexX =
                (int)(MathFunctions.RoundDownTo(CollisionPositionTopLeft.X, Level.GridCellSize) / Level.GridCellSize);
            int gridStartIndexY =
                (int)(MathFunctions.RoundDownTo(CollisionPositionTopLeft.Y, Level.GridCellSize) / Level.GridCellSize);

            float gridCellsAmountRawX = CollisionBoxSize.X + Math.Abs(CollisionPositionTopLeft.X % Level.GridCellSize);
            int gridCellsAmountX = (int)(MathFunctions.RoundUpTo(gridCellsAmountRawX, Level.GridCellSize) / Level.GridCellSize);
            float gridCellsAmountRawY = CollisionBoxSize.Y + Math.Abs(CollisionPositionTopLeft.Y % Level.GridCellSize);
            int gridCellsAmountY = (int)(MathFunctions.RoundUpTo(gridCellsAmountRawY, Level.GridCellSize) / Level.GridCellSize);

            Dictionary<Vector2, bool> intersectingGridCells = GetCollisionBoxData(gridStartIndexX, gridStartIndexY, gridCellsAmountX, gridCellsAmountY);

            while (shouldKeepResolving)
            {
                Vector2 positionDelta = Vector2.Zero;
                Vector2 velocityTemp = Vector2.Zero;
                Tuple<Vector2, Vector2> positionAndVelocityAdjustment = ProcessCollisionBoxData(intersectingGridCells);

                positionDelta = positionAndVelocityAdjustment.Item1;
                velocityTemp = positionAndVelocityAdjustment.Item2;

                if (Math.Abs(positionDelta.X) > Math.Abs(positionDelta.Y))
                {
                    positionDelta.Y = 0;
                    velocityTemp.Y = 0;
                    _isCollidingWithWall = true;
                    IsOnGround = false;
                }
                else
                {
                    if (positionDelta.Y < 0)
                    {
                        IsOnGround = true;
                    }
                    _isCollidingWithWall = false;
                    positionDelta.X = 0;
                    velocityTemp.X = 0;
                }
                _game.DrawDebugBox(CollisionPositionTopLeft, CollisionBoxSize, new Color(0, 255, 0));
                if (positionDelta != Vector2.Zero)
                {
                    Position += positionDelta;
                    Velocity += velocityTemp;
                }
                else
                {
                    shouldKeepResolving = false;
                }
            }
            DoCustomCollisionProcessing(intersectingGridCells, gridStartIndexX, gridStartIndexY, gridCellsAmountX, gridCellsAmountY);
        }

        private Dictionary<Vector2, bool> GetCollisionBoxData(int gridStartX, int gridStartY, int gridSizeX, int gridSizeY)
        {
            Dictionary<Vector2, bool> intersectingGridCells = new Dictionary<Vector2, bool>();

            for (int y = gridStartY; y < gridStartY + gridSizeY; y++)
            {
                for (int x = gridStartX; x < gridStartX + gridSizeX; x++)
                {
                    intersectingGridCells.Add(new Vector2(x, y), _level.GetTileIsSolid(x, y));
                }
            }
            return intersectingGridCells;
        }

        private Tuple<Vector2, Vector2> ProcessCollisionBoxData(Dictionary<Vector2, bool> collisionGrid)
        {
            Vector2 collisionPosition = Position + CollisionBoxOffset;

            Vector2 positionDelta = Vector2.Zero;
            Vector2 velocityTemp = Vector2.Zero;


            foreach (KeyValuePair<Vector2, bool> gridCell in collisionGrid)
            {
                Vector2 tileCenter = new Vector2(
                    gridCell.Key.X * Level.GridCellSize + (Level.GridCellSize / 2),
                    gridCell.Key.Y * Level.GridCellSize + (Level.GridCellSize / 2));
                Vector2 cellPosDelta = (collisionPosition + positionDelta) - tileCenter;
                float cellExtent, actorExtent, collisionSign, interpenetration;
                if (Math.Abs(cellPosDelta.X) >= Math.Abs(cellPosDelta.Y))
                {
                    if (cellPosDelta.X > 0)
                    {
                        cellExtent = tileCenter.X + (Level.GridCellSize / 2);
                        actorExtent = CollisionPositionTopLeft.X;
                        collisionSign = 1f;
                    }
                    else
                    {
                        cellExtent = tileCenter.X - (Level.GridCellSize / 2);
                        actorExtent = CollisionPositionBotRight.X;
                        collisionSign = -1f;
                    }
                }
                else
                {
                    if (cellPosDelta.Y > 0)
                    {
                        cellExtent = tileCenter.Y + (Level.GridCellSize / 2);
                        actorExtent = CollisionPositionTopLeft.Y;
                        collisionSign = 1f;
                    }
                    else
                    {
                        cellExtent = tileCenter.Y - (Level.GridCellSize / 2);
                        actorExtent = CollisionPositionBotRight.Y;
                        collisionSign = -1f;
                    }
                }
                if (gridCell.Value) //if the tile is solid
                {
                    interpenetration = (cellExtent - actorExtent) * collisionSign;
                    if (interpenetration > 0)
                    {
                        if (Math.Abs(cellPosDelta.X) >= Math.Abs(cellPosDelta.Y))
                        {
                            positionDelta.X = interpenetration * collisionSign;
                            velocityTemp.X = -Velocity.X;
                        }
                        else
                        {

                            positionDelta.Y = interpenetration * collisionSign;
                            velocityTemp.Y = -Velocity.Y;
                        }
                    }
                }

            }
            return new Tuple<Vector2, Vector2>(positionDelta, velocityTemp);
        }

        protected virtual void DoCustomCollisionProcessing(Dictionary<Vector2, bool> collisionGrid, int gridStartX, int gridStartY, int gridSizeX, int gridSizeY)
        {
        }

        //draw code

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,
                new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y)),
                //Position,
                null,
                Color.White,
                0f,
                new Vector2(_texture.Width / 2, _texture.Height / 2),
                1f,
                _direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
        }

        //event type functions

        public virtual void OnCollideWithOtherActor(Vector2 otherEntityPosition)
        {
            bool shouldDisableControls = true;
            Vector2 entityOffset = otherEntityPosition - (Position + CollisionBoxOffset);
            Vector2 newVelocity = Vector2.Zero;
            if (Math.Abs(entityOffset.X) > Math.Abs(entityOffset.Y))
            {
                newVelocity = new Vector2(1, -1);
                //if we are to the left of the other entity, recoil to the left instead of right
                if (entityOffset.X > 0)
                {
                    newVelocity.X = -1;
                }
            }
            else
            {
                if (entityOffset.Y > 0)
                {
                    newVelocity = new Vector2(0, -1);
                    shouldDisableControls = false;
                }
                else
                {
                }
            }
            newVelocity *= MotionProperties.CollisionRecoilSpeed;
            Velocity = newVelocity;

            if (shouldDisableControls)
            {
                ControlsEnabled = false;
                Timer.Create(MotionProperties.CollisionRecoilDuration, false, (timer) => ControlsEnabled = true);
                CollisionsEnabled = false;
                //change duration once i make combat properties
                Timer.Create(MotionProperties.CollisionRecoilDuration, false, (timer) => CollisionsEnabled = true);
            }
            else
            {
                CollisionsEnabled = false;
                //change duration once i make combat properties (this one should be shorter)
                Timer.Create(MotionProperties.CollisionRecoilDuration, false, (timer) => CollisionsEnabled = true);
            }
        }

        protected virtual void OnLostHealth(int oldHealth, int newHealth)
        {
        }

        //enums

        protected enum MoveDirection
        {
            Left = 0,
            None = 1,
            Right = 2
        }
    }

    public class ActorMotionProperties
    {
        public float RunAcceleration;
        public float RunJumpInitialAmount;
        public float RunJumpFloatAmount;

        public float WalkAcceleration;
        public float WalkJumpInitialAmount;
        public float WalkJumpFloatAmount;

        public float MaxWalkSpeed;
        public float MaxRunSpeed;
        public float MaxFallSpeed;

        public float HorizontalMovementDamping;
        public float CollisionHorizontalMovementDamping;

        public float CollisionRecoilSpeed;
        public float CollisionRecoilDuration;
        public float CollisionFallAcceleration;

        public float JumpFloatDecayAmount;
        public float DirectionFlipMinVelocity;
        public float FallAcceleration;

        public ActorMotionProperties()
        {
            WalkAcceleration = 0.03f;
            RunAcceleration = 0.05f;
            MaxWalkSpeed = 1f;
            MaxRunSpeed = 2f;
            WalkJumpInitialAmount = 0.35f;
            RunJumpInitialAmount = 0.5f;
            WalkJumpFloatAmount = 0.17f;
            RunJumpFloatAmount = 0.125f;
            JumpFloatDecayAmount = 0.96f;
            HorizontalMovementDamping = 0.125f;

            CollisionRecoilSpeed = 0.6f;
            CollisionRecoilDuration = 1.5f;
            CollisionFallAcceleration = 0.025f;
            CollisionHorizontalMovementDamping = 0.05f;

            DirectionFlipMinVelocity = 0.1f;
            MaxFallSpeed = 1.5f;
            FallAcceleration = 0.1f;
        }
    }
}
