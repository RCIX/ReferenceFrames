using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAHelpers;
using System.IO;

namespace ReferenceFrames
{
    class MovingPlatformEntity : MapEntity
    {
        private const float _moveSpeed = 24;
        private const float _directionChangeDelay = 0.5f;

        private float _travelTime;
        private Vector2 _minPos;
        private Vector2 _maxPos;
        private bool _movesOnYAxis = false;
        private bool _reversedDirection = false;
        private Interpolator _movementInterpolator;
        private Vector2 _lastPos;

        public MovingPlatformEntity(Vector2 positionStart, Vector2 positionEnd, MainGame game, string tilesetName)
            : base(positionStart, game)
        {

            _minPos = positionStart;
            _maxPos = positionEnd;
            Vector2 positionDelta = positionEnd - positionStart;
            if (Math.Abs(positionDelta.X) > Math.Abs(positionDelta.Y))
            {
                _travelTime = (float)Math.Abs(positionDelta.X / _moveSpeed);
                StartMovement();
                _texture = game.Content.Load<Texture2D>(Path.Combine("Tilesets", Path.Combine(tilesetName, "MovingPlatformHorizontal")));

            }
            else
            {
                _travelTime = (float)Math.Abs(positionDelta.Y / _moveSpeed);
                _movesOnYAxis = true;
                StartMovement();
                _texture = game.Content.Load<Texture2D>(Path.Combine("Tilesets", Path.Combine(tilesetName, "MovingPlatformVertical")));
            }
        }

        private static float SmoothStepMovement(float progress)
        {
            return MathHelper.SmoothStep(0, 1, progress);
        }

        private void StartMovement()
        {
            float posStart, posEnd;
            if (_movesOnYAxis)
            {
                posStart = _minPos.Y;
                posEnd = _maxPos.Y;
            }
            else
            {
                posStart = _minPos.X;
                posEnd = _maxPos.X;
            }
            if (_reversedDirection)
            {
                float temp;
                temp = posEnd;
                posEnd = posStart;
                posStart = temp;
            }
            _movementInterpolator = Interpolator.Create(
                posStart,
                posEnd,
                _travelTime,
                SmoothStepMovement,
                SetPosition,
                SetMoveTimer);
        }

        private void SetPosition(Interpolator interp)
        {
            _lastPos = Position;
            if (!_movesOnYAxis)
            {
                Position = new Vector2(interp.Value, Position.Y);
            }
            else
            {
                Position = new Vector2(Position.X, interp.Value);
            }
            Velocity = Position - _lastPos;
        }

        private void SetMoveTimer(Interpolator interp)
        {
            _reversedDirection = !_reversedDirection;
            Timer.Create(_directionChangeDelay, false, (timer) => StartMovement());
            _movementInterpolator.Stop();
            _movementInterpolator = null;
        }
    }
}
