using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Diagnostics;
using XNAHelpers;

namespace ReferenceFrames
{
    public class Level
    {
        public static float GridCellSize = 8;

        private static Dictionary<string, Texture2D> _tilesets;
        private static Dictionary<Tile, Vector2> _tileCoords;
        private static Dictionary<Tile, Func<Tile[,], bool>> _tileMatrixMatchers;
        private static Random _random;

        static Level()
        {
            _random = new Random();
            _tilesets = new Dictionary<string, Texture2D>();
            _tileCoords = new Dictionary<Tile, Vector2>();
            _tileMatrixMatchers = new Dictionary<Tile, Func<Tile[,], bool>>();

            _tileCoords.Add(Tile.FlatSurfaceUp, new Vector2(8, 0));
            _tileCoords.Add(Tile.FlatSurfaceDown, new Vector2(0, 0));
            _tileCoords.Add(Tile.FlatSurfaceLeft, new Vector2(0, 8));
            _tileCoords.Add(Tile.FlatSurfaceRight, new Vector2(8, 8));
            _tileCoords.Add(Tile.CornerUpLeft, new Vector2(16, 0));
            _tileCoords.Add(Tile.CornerUpRight, new Vector2(24, 0));
            _tileCoords.Add(Tile.CornerDownLeft, new Vector2(16, 8));
            _tileCoords.Add(Tile.CornerDownRight, new Vector2(24, 8));
            _tileCoords.Add(Tile.InsideUpLeft, new Vector2(0, 16));
            _tileCoords.Add(Tile.InsideUpRight, new Vector2(8, 16));
            _tileCoords.Add(Tile.InsideDownLeft, new Vector2(0, 24));
            _tileCoords.Add(Tile.InsideDownRight, new Vector2(8, 24));
            _tileCoords.Add(Tile.Solid, new Vector2(24, 24));
            _tileCoords.Add(Tile.Detail1, new Vector2(16, 24));
            _tileCoords.Add(Tile.Detail2, new Vector2(16, 16));
            _tileCoords.Add(Tile.Detail3, new Vector2(24, 16));
            //needs phasing out
            _tileCoords.Add(Tile.Platform, new Vector2(24, 24));

            _tileMatrixMatchers.Add(Tile.Solid, IsSolidTile);
            _tileMatrixMatchers.Add(Tile.FlatSurfaceUp, (tileMatrix) => IsFlatSurfaceTile(tileMatrix, 0));
            _tileMatrixMatchers.Add(Tile.FlatSurfaceDown, (tileMatrix) => IsFlatSurfaceTile(tileMatrix, 2));
            _tileMatrixMatchers.Add(Tile.FlatSurfaceLeft, (tileMatrix) => IsFlatSurfaceTile(tileMatrix, 1));
            _tileMatrixMatchers.Add(Tile.FlatSurfaceRight, (tileMatrix) => IsFlatSurfaceTile(tileMatrix, 3));
            _tileMatrixMatchers.Add(Tile.CornerUpLeft, (tileMatrix) => IsCornerTile(tileMatrix, 0));
            _tileMatrixMatchers.Add(Tile.CornerUpRight, (tileMatrix) => IsCornerTile(tileMatrix, 1));
            _tileMatrixMatchers.Add(Tile.CornerDownLeft, (tileMatrix) => IsCornerTile(tileMatrix, 2));
            _tileMatrixMatchers.Add(Tile.CornerDownRight, (tileMatrix) => IsCornerTile(tileMatrix, 3));
            _tileMatrixMatchers.Add(Tile.InsideUpLeft, (tileMatrix) => IsInsideTile(tileMatrix, 0));
            _tileMatrixMatchers.Add(Tile.InsideUpRight, (tileMatrix) => IsInsideTile(tileMatrix, 1));
            _tileMatrixMatchers.Add(Tile.InsideDownLeft, (tileMatrix) => IsInsideTile(tileMatrix, 2));
            _tileMatrixMatchers.Add(Tile.InsideDownRight, (tileMatrix) => IsInsideTile(tileMatrix, 3));
        }

        private static bool IsSolidTile(Tile[,] tileMatrix)
        {
            if (tileMatrix[0, 0] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[1, 0] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[2, 0] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[0, 1] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[2, 1] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[0, 2] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[1, 2] == Tile.Air)
            {
                return false;
            }
            if (tileMatrix[2, 2] == Tile.Air)
            {
                return false;
            }
            return true;
        }

        private static bool IsFlatSurfaceTile(Tile[,] tileMatrix, int direction)
        {
            //0 = up
            //1 = left
            //2 = down
            //3 = right
            if (tileMatrix[1, 0] == Tile.Solid &&
                tileMatrix[0, 1] == Tile.Solid &&
                tileMatrix[1, 2] == Tile.Solid &&
                tileMatrix[2, 1] == Tile.Solid)
            {
                return false;
            }
            if (tileMatrix[1, 0] == Tile.Air && direction != 0)
            {
                return false;
            }
            if (tileMatrix[0, 1] == Tile.Air && direction != 1)
            {
                return false;
            }
            if (tileMatrix[1, 2] == Tile.Air && direction != 2)
            {
                return false;
            }
            if (tileMatrix[2, 1] == Tile.Air && direction != 3)
            {
                return false;
            }
            return true;
        }

        private static bool IsCornerTile(Tile[,] tileMatrix, int direction)
        {
            //0 is up/left
            //1 is up/right
            //2 is down/left
            //3 is down/right
            if (tileMatrix[1, 0] == Tile.Solid && (direction == 0 || direction == 1) ||
                tileMatrix[1, 0] != Tile.Solid && (direction == 2 || direction == 3))
            {
                return false;
            }
            if (tileMatrix[0, 1] == Tile.Solid && (direction == 0 || direction == 2) ||
                tileMatrix[0, 1] != Tile.Solid && (direction == 1 || direction == 3))
            {
                return false;
            }
            if (tileMatrix[2, 1] == Tile.Solid && (direction == 1 || direction == 3) ||
                tileMatrix[2, 1] != Tile.Solid && (direction == 2 || direction == 4))
            {
                return false;
            }
            if (tileMatrix[1, 2] == Tile.Solid && (direction == 2 || direction == 3) ||
                tileMatrix[1, 2] != Tile.Solid && (direction == 1 || direction == 4))
            {
                return false;
            }
            return true;
        }

        private static bool IsInsideTile(Tile[,] tileMatrix, int direction)
        {
            //0 is up/left
            //1 is up/right
            //2 is down/left
            //3 is down/right

            //check if corner tiles are solid
            if (tileMatrix[0, 0] == Tile.Solid && direction == 0)
            {
                return false;
            }
            if (tileMatrix[2, 0] == Tile.Solid && direction == 1)
            {
                return false;
            }
            if (tileMatrix[0, 2] == Tile.Solid && direction == 2)
            {
                return false;
            }
            if (tileMatrix[2, 2] == Tile.Solid && direction == 3)
            {
                return false;
            }

            //check if appropriate adjacent tiles are solid
            if (tileMatrix[1, 0] != Tile.Solid && (direction == 0 || direction == 1))
            {
                return false;
            }
            if (tileMatrix[0, 1] != Tile.Solid && (direction == 0 || direction == 2))
            {
                return false;
            }
            if (tileMatrix[2, 1] != Tile.Solid && (direction == 1 || direction == 3))
            {
                return false;
            }
            if (tileMatrix[1, 2] != Tile.Solid && (direction == 2 || direction == 3))
            {
                return false;
            }
            return true;
        }

        public static void AddTileset(string tilesetName, ContentManager content)
        {
            if (!_tilesets.ContainsKey(tilesetName))
            {
                Texture2D tileset = content.Load<Texture2D>(Path.Combine("Tilesets", Path.Combine(tilesetName, "Terrain")));
                _tilesets.Add(tilesetName, tileset);
            }
        }

        private Tile[,] _tiles;
        private bool[,] _accessedTiles;
        private string _tilesetName;
        private MainGame _game;

        public List<MapEntity> Entities { get; set; }
        public List<PowerupEntity> Powerups { get; set; }
        public List<ActorEntity> Enemies { get; set; }
        public Vector2 PlayerSpawn { get; set; }
        //expressed in tiles
        public Vector2 LevelSize { get; set; }

        public Level(Texture2D levelInfo, string tilesetName, MainGame game)
        {
            _game = game;
            _tiles = new Tile[levelInfo.Width, levelInfo.Height];
            _tilesetName = tilesetName;
            LevelSize = new Vector2(levelInfo.Width, levelInfo.Height);

            Entities = new List<MapEntity>();
            Powerups = new List<PowerupEntity>();
            Enemies = new List<ActorEntity>();

            //replace with powerup parsing code
            Powerups.Add(new SwordPowerupEntity(new Vector2(0, 2), game));
            

            ParseLevelTexture(levelInfo);
        }

        public void SetPlayerForPowerups(PlayerEntity player)
        {
            foreach (PowerupEntity powerup in Powerups)
            {
                powerup.Player = player;
            }
        }

        private void ParseLevelTexture(Texture2D levelInfo)
        {
            Tile[,] tilesTemp = new Tile[levelInfo.Width, levelInfo.Height];
            _accessedTiles = new bool[levelInfo.Width, levelInfo.Height];

            Color[] levelColors = new Color[levelInfo.Width * levelInfo.Height];
            levelInfo.GetData<Color>(levelColors);

            Dictionary<int, Vector2> movingPlatformStarts = new Dictionary<int, Vector2>();
            Dictionary<int, Vector2> movingPlatformEnds = new Dictionary<int, Vector2>();

            for (int x = 0; x < levelInfo.Width; x++)
            {
                for (int y = 0; y < levelInfo.Height; y++)
                {
                    Tile currentCoordsTile;
                    if (!GetTileType(levelColors[x + (y * levelInfo.Width)], out currentCoordsTile))
                    {
                        currentCoordsTile = Tile.Air;
                        //we have an entity or special code on our hands, figure out what kind it is and make one
                        Color entityColor = levelColors[x + (y * levelInfo.Width)];
                        Vector2 tileWorldLocation = new Vector2(x * 8 + 4, y * 8 + 4);
                        if (entityColor.R == 0 && entityColor.G == 255 && entityColor.B == 0)
                        {
                            PlayerSpawn = tileWorldLocation;
                        }

                        if (entityColor.R == 102 && entityColor.G > 0)
                        {
                            if (entityColor.B == 128)
                            {
                                if (!movingPlatformStarts.ContainsKey(entityColor.G))
                                {
                                    movingPlatformStarts.Add(entityColor.G, tileWorldLocation);
                                }
                            }
                            else
                            {
                                if (!movingPlatformEnds.ContainsKey(entityColor.G))
                                {
                                    movingPlatformEnds.Add(entityColor.G, tileWorldLocation);
                                }
                            }
                        }

                        if (entityColor.R == 153)
                        {
                            Entities.Add(new PositionAnchorEntity(new Vector2(x * 8 + 4, y * 8 + 4), _game, _tilesetName));
                        }
                    }
                    _tiles[x, y] = currentCoordsTile;
                    tilesTemp[x, y] = currentCoordsTile;
                }
            }

            //this method automatically ignores ends that don't have a start defined
            foreach (KeyValuePair<int, Vector2> idPosition in movingPlatformStarts)
            {
                if (!movingPlatformEnds.ContainsKey(idPosition.Key))
                {
                    //an unmatched pair, skip
                    continue;
                }
                Entities.Add(new MovingPlatformEntity(idPosition.Value, movingPlatformEnds[idPosition.Key], _game, _tilesetName));
            }

            tilesTemp = ProcessGroundTiles(tilesTemp);

            _tiles = tilesTemp;
        }

        private Tile[,] ProcessGroundTiles(Tile[,] baseTiles)
        {
            //checks against _tiles in this function are intended, to avoid having to use a lot more effort to figure out whether a tile is a solid one or not
            for (int x = 0; x < baseTiles.GetLength(0); x++)
            {
                for (int y = 0; y < baseTiles.GetLength(1); y++)
                {
                    if (_tiles[x, y] == Tile.Solid)
                    {
                        //build list of surrounding tiles
                        //run through checker to get proper output
                        Tile[,] surroundingTiles = new Tile[3, 3];
                        surroundingTiles[0, 0] = Tile.Air;
                        surroundingTiles[1, 0] = Tile.Air;
                        surroundingTiles[2, 0] = Tile.Air;

                        surroundingTiles[0, 1] = Tile.Air;
                        surroundingTiles[1, 1] = Tile.Solid;
                        surroundingTiles[2, 1] = Tile.Air;

                        surroundingTiles[0, 2] = Tile.Air;
                        surroundingTiles[1, 2] = Tile.Air;
                        surroundingTiles[2, 2] = Tile.Air;

                        bool noLeft = false;
                        bool noRight = false;
                        bool noTop = false;
                        bool noBot = false;

                        if (x == 0) //if we're at left edge, all tiles to the left are assumed to be solid
                        {
                            surroundingTiles[0, 0] = Tile.Solid;
                            surroundingTiles[0, 1] = Tile.Solid;
                            surroundingTiles[0, 2] = Tile.Solid;
                            noLeft = true;
                        }
                        if (x == _tiles.GetLength(0) - 1) //if we're at right edge, all tiles to the right are assumed to be solid
                        {
                            surroundingTiles[2, 0] = Tile.Solid;
                            surroundingTiles[2, 1] = Tile.Solid;
                            surroundingTiles[2, 2] = Tile.Solid;
                            noRight = true;
                        }
                        if (y == 0) //if we're at top edge, all tiles above are assumed to be solid
                        {
                            surroundingTiles[0, 0] = Tile.Solid;
                            surroundingTiles[1, 0] = Tile.Solid;
                            surroundingTiles[2, 0] = Tile.Solid;
                            noTop = true;
                        }
                        if (y == _tiles.GetLength(1)- 1) //if we're at bottom edge, all tiles below are assumed to be solid
                        {
                            surroundingTiles[0, 2] = Tile.Solid;
                            surroundingTiles[1, 2] = Tile.Solid;
                            surroundingTiles[2, 2] = Tile.Solid;
                            noBot = true;
                        }
                        if (!noLeft && !noTop)
                        {
                            surroundingTiles[0, 0] = _tiles[x - 1, y - 1] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noTop)
                        {
                            surroundingTiles[1, 0] = _tiles[x, y - 1] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noTop && !noRight)
                        {
                            surroundingTiles[2, 0] = _tiles[x + 1, y - 1] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noLeft)
                        {
                            surroundingTiles[0, 1] = _tiles[x - 1, y] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noRight)
                        {
                            surroundingTiles[2, 1] = _tiles[x + 1, y] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noLeft && !noBot)
                        {
                            surroundingTiles[0, 2] = _tiles[x - 1, y + 1] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noBot)
                        {
                            surroundingTiles[1, 2] = _tiles[x, y + 1] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        if (!noRight && !noBot)
                        {
                            surroundingTiles[2, 2] = _tiles[x + 1, y + 1] == Tile.Solid ? Tile.Solid : Tile.Air;
                        }
                        baseTiles[x, y] = CalculateGroundTileType(surroundingTiles);
                    }
                }
            }
            return baseTiles;
        }

        private Tile CalculateGroundTileType(Tile[,] tileMatrix)
        {
            foreach (KeyValuePair<Tile, Func<Tile[,], bool>> matrixMatcher in _tileMatrixMatchers)
            {
                if (matrixMatcher.Value(tileMatrix))
                {
                    return matrixMatcher.Key;
                }
            }
            return Tile.Solid;
        }

        private bool GetTileType(Color color, out Tile result)
        {
            result = Tile.Air;
            switch (color.R)
            {
                case 0:
                    if (color.G != 0 || color.B != 0)
                    {
                        return false;
                    }
                    break;
                case 51: //ground
                    result = Tile.Solid;
                    break;
                case 102: //platform
                    if (color.G == 0) //check to see if it's a stationary one
                    {
                        result = Tile.Platform;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case 153: //position frame
                    return false;
                case 204:
                    int detailType = _random.Next(3); //calculate a random detail type
                    result = (Tile)(13 + detailType);
                    break;
                case 255: //hazards
                    if (color.G == 0)
                    {
                        result = Tile.Hazard1;
                    }
                    else if (color.G == 128)
                    {
                        result = Tile.Hazard2;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        private void DrawSingleTile(SpriteBatch spriteBatch, Tile tile, Vector2 coordinates)
        {
            Rectangle sourceRectangle = new Rectangle();
            sourceRectangle.X = (int)_tileCoords[tile].X;
            sourceRectangle.Y = (int)_tileCoords[tile].Y;
            sourceRectangle.Width = 8;
            sourceRectangle.Height = 8;
            spriteBatch.Draw(_tilesets[_tilesetName], coordinates, sourceRectangle, Color.White);
        }

        public bool GetTileIsSolid(int x, int y)
        {
            if (x < 0 || x > _tiles.GetLength(0) - 1)
            {
                return true;
            }
            if (y < 0 || y > _tiles.GetLength(1) - 1)
            {
                return false;
            }
            _accessedTiles[x, y] = true;
            return (int)_tiles[x, y] < 13 || (int)_tiles[x, y] == 16;
        }

        public PowerupEntity GetTileHasPowerup(int x, int y)
        {
            Vector2 position = new Vector2(x, y);
            foreach (PowerupEntity powerup in Powerups)
            {
                if (powerup.Position == position)
                {
                    return powerup;
                }
            }
            return null;
        }

        public void Update(GameTime elapsedTime)
        {
            foreach (MapEntity ent in Entities)
            {
                ent.Update(elapsedTime);
            }
            foreach (ActorEntity enemy in Enemies)
            {
                enemy.Update(elapsedTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    
                    if (_accessedTiles[x, y])
                    {
                        _game.DrawDebugBox(
                            (new Vector2(x, y) * GridCellSize) + new Vector2(0.5f), 
                            new Vector2(GridCellSize - 0.5f), 
                            Color.Yellow);
                    }
                    else
                    {
                        _game.DrawDebugBox(
                            (new Vector2(x, y) * GridCellSize) + new Vector2(0.5f), 
                            new Vector2(GridCellSize - 0.5f), 
                            Color.Red);
                    }
                    _accessedTiles[x, y] = false;
                    if (_tiles[x, y] != Tile.Air)
                    {
                        DrawSingleTile(spriteBatch, _tiles[x, y], new Vector2(x * 8, y * 8));
                    }
                }
            }
            foreach (MapEntity entity in Entities)
            {
                entity.Draw(spriteBatch);
            }
            foreach (PowerupEntity powerup in Powerups)
            {
                powerup.Draw(spriteBatch);
            }
            foreach (ActorEntity enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }
    }

    public enum Tile
    {
        FlatSurfaceUp = 0,
        FlatSurfaceDown = 1,
        FlatSurfaceLeft = 2,
        FlatSurfaceRight = 3,
        CornerUpLeft = 4,
        CornerUpRight = 5,
        CornerDownLeft = 6,
        CornerDownRight = 7,
        InsideUpLeft = 8,
        InsideUpRight = 9,
        InsideDownLeft = 10,
        InsideDownRight = 11,
        Solid = 12,
        Detail1 = 13,
        Detail2 = 14,
        Detail3 = 15,
        Platform = 16,
        Hazard1 = 17,
        Hazard2 = 18,
        Air = 19

    }
}
