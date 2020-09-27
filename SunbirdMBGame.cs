using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using Sunbird.Core;
using SunbirdMB.Core;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;

namespace SunbirdMB
{
    public class SunbirdMBGame : WpfGame, IMainGame
    {
        private IGraphicsDeviceService graphicsDeviceManager;
        private IServiceContainer services;
        private SpriteBatch spriteBatch;
        private State currentState;

        public State CurrentState
        {
            get { return currentState; }
            set
            {
                if (currentState != null && currentState != value) { currentState.OnStateChanged(); }
                currentState = value;
            }
        }

        public int BackBufferWidth { get { return GraphicsDevice.PresentationParameters.BackBufferWidth; } }
        public int BackBufferHeight { get { return GraphicsDevice.PresentationParameters.BackBufferHeight; } }
        public SamplerState SamplerState { get; set; } = SamplerState.PointWrap;
        public Camera Camera { get; set; }
        public Keyboard Keyboard { get; set; }
        public Mouse Mouse { get; set; }

        public bool cleanLoad = false;

        //public event EventHandler Loaded;

        protected override void Initialize()
        {
            // must be initialized. required by Content loading and rendering (will add itself to the Services)
            // note that MonoGame requires this to be initialized in the constructor, while WpfInterop requires it to
            // be called inside Initialize (before base.Initialize())
            graphicsDeviceManager = new WpfGraphicsDeviceService(this);

            // wpf and keyboard need reference to the host control in order to receive input
            // this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
            Keyboard = new Keyboard(this);
            Mouse = new Mouse(this);

            services = new ServiceContainer();
            services.AddService(typeof(IGraphicsDeviceService), graphicsDeviceManager);
            Content = new ContentManager(services);

            Serializer.ExtraTypes = new Type[]
            {
                typeof(Cube),
                typeof(Deco),
                typeof(GhostMarker),
            };

            Camera = new Camera(this);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // must be called after the WpfGraphicsDeviceService instance was created
            base.Initialize();

            // content loading now possible

            Content.RootDirectory = "Content";

            CurrentState = new MapBuilder(this, GraphicsDevice, Content, "MapBuilderSave.xml");

        }

        internal void SaveAndSerialize()
        {
            CurrentState.SaveAndSerialize();
        }

        /// <summary>
        /// Set the width and height of the backbuffer to the wpf panel that hosts the main game.
        /// </summary>
        public void SetCameraTransformMatrix(int width, int height)
        {
            GraphicsDevice.PresentationParameters.BackBufferWidth = width;
            GraphicsDevice.PresentationParameters.BackBufferHeight = height;
            if (Camera.CurrentMode == CameraMode.Drag)
            {
                Camera.RecreateDragTransform();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            Peripherals.PreUpdate(this);

            CurrentState.Update(gameTime);
            Camera.Update(this);

            Peripherals.PostUpdate();
        }

        protected override void Draw(GameTime gameTime)
        {
            var wpfRenderTarget = (RenderTarget2D)GraphicsDevice.GetRenderTargets()[0].RenderTarget;
            GraphicsDevice.SetRenderTarget(wpfRenderTarget);
            GraphicsDevice.Clear(GraphicsHelper.HexColor("#1e1e1e"));
            spriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
            CurrentState.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }
    }
}
