using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;

namespace ReferenceFrames
{
    class AnimatedSprite
    {
        private Dictionary<string, SpriteAnimation> _sprites;
        public Texture2D CurrentFrameTexture
        {
            get
            {
                return _sprites[_currentAnimation].CurrentFrameTexture;
            }
        }
        private string _currentAnimation;

        public AnimatedSprite(string baseDirectory, MainGame game)
        {
            _sprites = new Dictionary<string, SpriteAnimation>();

            using (StreamReader animData = new StreamReader(
                Path.Combine(
                    Assembly.GetExecutingAssembly().Location, 
                    Path.Combine(
                        baseDirectory, 
                        "animationData.txt"))))
            {
                List<string> lines = new List<string>();
                while (!animData.EndOfStream)
                {
                    lines.Add(animData.ReadLine());
                }

                foreach (string line in lines)
                {
                    string[] animNameAndSpriteName = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    SpriteAnimation sprite = new SpriteAnimation(Path.Combine(baseDirectory, animNameAndSpriteName[1]), game);
                    _sprites.Add(animNameAndSpriteName[0], sprite);
                    if (animNameAndSpriteName.Length > 2)
                    {
                        if (animNameAndSpriteName[2] == "default")
                        {
                            _currentAnimation = animNameAndSpriteName[0];
                        }
                    }
                }
            }
        }

        public void SetAnimation(string name)
        {
            if (_sprites.ContainsKey(name))
            {
                _sprites[_currentAnimation].IsAnimating = false;
                _sprites[_currentAnimation].Reset();
                _currentAnimation = name;
                _sprites[_currentAnimation].IsAnimating = true;
            }
        }

        public void Update(GameTime elapsedTime)
        {
            _sprites[_currentAnimation].Update(elapsedTime);
        }
    }
}
