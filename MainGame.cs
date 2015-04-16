using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using XNAHelpers;

namespace ReferenceFrames
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        public static float EpsilonFloat = 0.001f;
        public static Vector2 ScreenSize = new Vector2(512, 512);

        public InputHelper Input { get; set; }
        public Level CurrentLevel { get; set; }
        public Camera2D Camera { get; set; }
        public MapEntity ClosestEntity { get; set; }

        private LineDrawer _debugLineDrawer;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PlayerUI _playerUI;
        private PlayerEntity _player;
        private bool _noPostProcessing;

        private RenderTarget2D _screenRenderTarget;
        private RetroGfxRenderer _retroRenderer;

        public MainGame(bool noPostProcessing)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = (int)ScreenSize.X;
            _graphics.PreferredBackBufferHeight = (int)ScreenSize.Y;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
            Window.Title = "Reference Frame";
            _noPostProcessing = noPostProcessing;
        }

        protected override void Initialize()
        {
            Input = new InputHelper();
            base.Initialize();
        }

        private void ResetPlayer()
        {
            _player.Position = CurrentLevel.PlayerSpawn;
            _player.Velocity = Vector2.Zero;
            _player.IsResetting = false;
            _player.RemovePowerup();
            _player.CurrentEnergy = 0;
            _player.CurrentHealth = _player.MaxHealth;
        }

        public void OnPlayerDied()
        {
            if (_player.IsResetting == false)
            {
                _player.IsResetting = true;
                Timer.Create(0.5f, false, (timer) => ResetPlayer());
            }
        }

        private void FindClosestMapEntity()
        {
            float closestDistance = float.MaxValue;
            int closestIndex = -1;
            ClosestEntity = null;
            for (int i = 0; i < CurrentLevel.Entities.Count; i++)
            {
                MapEntity ent = CurrentLevel.Entities[i];
                ent.IsClosestReferenceFrame = false;
                Vector2 distance = ent.Position - _player.Position;
                bool shouldProcessThisEntity = true;
                if (_player.SelectedMapEntity is PositionAnchorEntity)
                {
                    if (!(ent is PositionAnchorEntity))
                    {
                        shouldProcessThisEntity = false;
                    }
                }
                if (!Camera.IsInCameraView(ent.Position))
                {
                    shouldProcessThisEntity = false;
                }

                if (distance.Length() < closestDistance && !ent.IsSelectedReferenceFrame && shouldProcessThisEntity)
                {
                    closestDistance = distance.Length();
                    closestIndex = i;
                }
            }
            if (closestIndex != -1)
            {
                CurrentLevel.Entities[closestIndex].IsClosestReferenceFrame = true;
                ClosestEntity = CurrentLevel.Entities[closestIndex];
            }
            else
            {
                ClosestEntity = null;
            }
        }

        protected override void LoadContent()
        {
            SetUpRenderer();

            _screenRenderTarget = new RenderTarget2D(
                GraphicsDevice, 
                (int)(ScreenSize.X / 2), 
                (int)(ScreenSize.Y / 4), 
                1, 
                GraphicsDevice.PresentationParameters.BackBufferFormat);
            Camera = new Camera2D(new Vector2(128));
            Camera.TrackingFunction = () => _player.Position;
            Camera.VerticalCameraMovement = (input, camera) => 0;
            Camera.HorizontalCameraMovement = (input, camera) => 0;
            Camera.ZoomIn = (input) => false;
            Camera.ZoomOut = (input) => false;
            Camera.SmoothMovement = false;

            Level.AddTileset("Grass", Content);
            CurrentLevel = new Level(Content.Load<Texture2D>("Levels/Test Level"), "Grass", this);
            CurrentLevel.Enemies.Add(new BasicEnemyEntity(new Vector2(4, 116), this));


            _player = new PlayerEntity(CurrentLevel.PlayerSpawn, this);
            _playerUI = new PlayerUI(this, _spriteBatch, _player);
            Camera.MinPosition = new Vector2(64, 64);
            Camera.MaxPosition = new Vector2(
                (CurrentLevel.LevelSize.X * 8) - 64,
                (CurrentLevel.LevelSize.Y * 8) - 64);

            CurrentLevel.SetPlayerForPowerups(_player);
            _debugLineDrawer = new LineDrawer(GraphicsDevice, _spriteBatch);
        }

        private void SetUpRenderer()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _retroRenderer = new RetroGfxRenderer(GraphicsDevice, _spriteBatch, Content,
                "RetroFX/GlowShader", "RetroFX/Pixel", "RetroFX/Scanlines", null);
            _retroRenderer.BloomThreshhold = 0.85f;
            _retroRenderer.BloomOverlayPercent = 0.75f;
            _retroRenderer.BloomPrimaryPixelPercent = 0.27f;
            _retroRenderer.BloomSecondaryPixelPercent = 0.22f;
            _retroRenderer.BloomIterations = 6f;
            if (_noPostProcessing)
            {
                _retroRenderer.SkipPostProcessing = true;
            }
        }

        public void DrawDebugBox(Vector2 position, Vector2 size, Color color)
        {
#if DEBUG
            Vector2 topLeftPos = new Vector2(
                position.X * 4,
                position.Y * 4);
            Vector2 topRightPos = new Vector2(
                (position.X + size.X) * 4,
                position.Y * 4);
            Vector2 botLeftPos = new Vector2(
                position.X * 4,
                (position.Y + size.Y) * 4);
            Vector2 botRightPos = new Vector2(
                (position.X + size.X) * 4,
                (position.Y + size.Y) * 4);
            _debugLineDrawer.QueueLine(new Line2D
            {
                Start = topLeftPos,
                End = topRightPos,
                Color = color,
            });
            _debugLineDrawer.QueueLine(new Line2D
            {
                Start = topLeftPos,
                End = botLeftPos,
                Color = color,
            });
            _debugLineDrawer.QueueLine(new Line2D
            {
                Start = topRightPos,
                End = botRightPos,
                Color = color,
            });
            _debugLineDrawer.QueueLine(new Line2D
            {
                Start = botLeftPos,
                End = botRightPos,
                Color = color,
            });
#endif
        }

        public void DrawDebugLine(Vector2 start, Vector2 end, Color color)
        {
#if DEBUG
            _debugLineDrawer.QueueLine(new Line2D
            {
                Start = start * 4,
                End = end * 4,
                Color = color,
            });
#endif
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();
            Interpolator.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            Timer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (Input.ExitRequested)
                this.Exit();
            if (Input.IsNewPress(Keys.V))
            {
                _retroRenderer.TextureMode++;
                if (_retroRenderer.TextureMode > TextureRenderingMode.BaseOnly)
                {
                    _retroRenderer.TextureMode = TextureRenderingMode.Both;
                }
            }
            if (Input.IsNewPress(Keys.A))
            {
                _player.MaxHealth++;
            }
            if (Input.IsNewPress(Keys.S))
            {
                _player.MaxEnergy++;
            }
            CurrentLevel.Update(gameTime);
            FindClosestMapEntity();
            _player.Update(gameTime);
            Camera.Update(Input);
            base.Update(gameTime);
            if (_player.Position.Y > CurrentLevel.LevelSize.Y * 8 )
            {
                OnPlayerDied();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            DepthStencilBuffer baseStencilBuffer = GraphicsDevice.DepthStencilBuffer;
            GraphicsDevice.DepthStencilBuffer = null;
            RenderTarget2D backBuffer = (RenderTarget2D)GraphicsDevice.GetRenderTarget(0);
            GraphicsDevice.SetRenderTarget(0, _screenRenderTarget);
            GraphicsDevice.Clear(new Color(80, 120, 180, 255));
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState, Camera.CameraMatrix);
            GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Point;
            GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Point;
            GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Point;
            CurrentLevel.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);
            _spriteBatch.End();


            _spriteBatch.Begin();
            _playerUI.Draw();
            _spriteBatch.End();
            

            GraphicsDevice.SetRenderTarget(0, backBuffer);
            Texture2D baseScreen = _screenRenderTarget.GetTexture();
            Texture2D retroScreen = _retroRenderer.RenderEffect(baseScreen, backBuffer);
            GraphicsDevice.SetRenderTarget(0, backBuffer);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _spriteBatch.Draw(retroScreen, Vector2.Zero, Color.White);
            _spriteBatch.End();

#if DEBUG
            _debugLineDrawer.Draw(true, Matrix.CreateTranslation(new Vector3(-(Camera.Position * 4), 0)) * Matrix.CreateTranslation(new Vector3((Camera.Size * 4) / 2, 0)));
#endif

            base.Draw(gameTime);
        }
    }
}
