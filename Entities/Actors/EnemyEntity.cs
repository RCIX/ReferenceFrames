using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ReferenceFrames
{
    class EnemyEntity : ActorEntity
    {
        public int HealthCostOnCollision { get; set; }

        public EnemyEntity(Vector2 position, MainGame game)
            : base(position, game)
        {
            HealthCostOnCollision = 1;
        }

        //custom override on this to perform checks for events
        protected override void DoCustomCollisionProcessing(Dictionary<Vector2, bool> collisionGrid, int gridStartX, int gridStartY, int gridSizeX, int gridSizeY)
        {
            //note: currently expects collisionGrid to be in a sequential row format
            //List<Vector2> collisionGridIndexes = collisionGrid.Keys.ToList();
            List<bool> collisionGridSolidFlags = collisionGrid.Values.ToList();
            List<bool> wallTiles = new List<bool>();

            int wallIndexX = gridStartX + (gridSizeX - 1);
            if (_moveDirection == MoveDirection.Left)
            {
                wallIndexX = gridStartX;
            }
            for (int y = gridStartY; y < gridStartY + gridSizeY; y++)
            {
                wallTiles.Add(collisionGrid[new Vector2(wallIndexX, y)]);
            }
            bool wallEventFlag = CheckForWallCollision(wallTiles, gridSizeY);
            bool edgeEventFlag = CheckForEdgeInCurrentDirection(collisionGrid, gridSizeX, gridSizeY, gridStartX, gridStartY);
            if (!IsOnGround)
            {
                if (wallEventFlag)
                {
                    OnReachedWall();
                }
            }
            else
            {
                if (wallEventFlag)
                {
                    OnReachedWall();
                }
                else
                {
                    if (edgeEventFlag)
                    {
                        OnReachedEdge();
                    }
                }
            }
        }

        private bool CheckForEdgeInCurrentDirection(Dictionary<Vector2, bool> collisionGrid, int gridSizeX, int gridSizeY, int gridStartX, int gridStartY)
        {
            if (_moveDirection == MoveDirection.None)
            {
                return false;
            }
            Vector2 cornerCellToCheck = Vector2.Zero;
            if (_moveDirection == MoveDirection.Left)
            {
                cornerCellToCheck = new Vector2(gridStartX, gridStartY + gridSizeY - 1);
            }
            else if (_moveDirection == MoveDirection.Right)
            {
                cornerCellToCheck = new Vector2(gridStartX + gridSizeX - 1, gridStartY + gridSizeY - 1);
            }
            return !collisionGrid[cornerCellToCheck];
        }

        private bool CheckForWallCollision(List<bool> wallTiles, int gridSizeY)
        {
            int numberOfWallTilesFound = 0;
            for (int i = wallTiles.Count - 1; i >= 0; i--)
            {
                if (wallTiles[i])
                {
                    numberOfWallTilesFound++;
                }
            }
            //Math.Min is so that if you have a collision box smaller than 1 tile, then this code will still fire this event in some corner cases
            if (numberOfWallTilesFound >= Math.Min(gridSizeY, 2))
            {
                return true;
            }
            return false;
        }

        protected virtual void OnReachedEdge()
        {
        }
        protected virtual void OnReachedWall()
        {
        }
    }
}
