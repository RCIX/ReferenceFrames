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
    class BasicEnemyEntity : EnemyEntity
    {
        public BasicEnemyEntity(Vector2 position, MainGame game)
            : base(position, game)
        {
            _texture = game.Content.Load<Texture2D>(Path.Combine("Enemies", Path.Combine("Basic", "Enemy")));
            _moveDirection = MoveDirection.Right;
            MotionProperties.WalkAcceleration = 0.35f;
            MotionProperties.RunAcceleration = 0.7f;
        }

        protected override void OnReachedEdge()
        {
            ReverseMoveDirection();
            //_isJumping = true;
            //Timer.Create(1f, false, (timer) => _isJumping = false);
        }

        public override void OnCollideWithOtherActor(Vector2 otherEntityPosition)
        {
            ReverseMoveDirection();
            base.OnCollideWithOtherActor(otherEntityPosition);
        }

        protected override void OnReachedWall()
        {
            ReverseMoveDirection();
        }

        private void ReverseMoveDirection()
        {
            if (_moveDirection == MoveDirection.Left)
            {
                _moveDirection = MoveDirection.Right;
            }
            else if (_moveDirection == MoveDirection.Right)
            {
                _moveDirection = MoveDirection.Left;
            }
        }

    }
}
