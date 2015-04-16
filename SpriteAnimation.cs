using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework;

namespace ReferenceFrames
{
    class SpriteAnimation
    {
        public bool IsAnimating { get; set; }
        public Texture2D CurrentFrameTexture
        {
            get
            {
                return _textures[_currentFrame];
            }
        }

        private List<Texture2D> _textures = new List<Texture2D>();
        private List<float> _textureFrameTimes = new List<float>();
        private int _currentFrame = 0;

        private float _currentFrameTime = 0;

        public SpriteAnimation(string baseDir, MainGame game)
        {
            IsAnimating = true;
            List<string> metadataLines = new List<string>();
            using (StreamReader metadata = new StreamReader(Path.Combine(Assembly.GetExecutingAssembly().Location, Path.Combine(baseDir, "metadata.txt"))))
            {
                while (!metadata.EndOfStream)
                {
                    metadataLines.Add(metadata.ReadLine());
                }
            }
            foreach (string tex in metadataLines)
            {
                string[] texAndFrameTime = tex.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                _textureFrameTimes.Add(Convert.ToSingle(texAndFrameTime[1]));
                _textures.Add(game.Content.Load<Texture2D>(Path.Combine(baseDir, texAndFrameTime[0])));
            }
        }

        public void Update(GameTime elapsedTime)
        {
            _currentFrameTime += (float)elapsedTime.ElapsedGameTime.TotalSeconds;
            if (_currentFrameTime > _textureFrameTimes[_currentFrame])
            {
                _currentFrameTime = 0;
                _currentFrame++;
                if (_currentFrame >= _textureFrameTimes.Count)
                {
                    _currentFrame = 0;
                }
            }
        }

        public void Reset()
        {
            _currentFrame = 0;
            _currentFrameTime = 0;
        }
    }
}
