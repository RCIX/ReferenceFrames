using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace ReferenceFrames
{
    class PositionAnchorEntity : MapEntity
    {
        public PositionAnchorEntity(Vector2 position, MainGame game, string tilesetName): base(position, game)
        {
            _texture = game.Content.Load<Texture2D>(Path.Combine("Tilesets", Path.Combine(tilesetName, "PositionAnchor")));
        }
    }
}
