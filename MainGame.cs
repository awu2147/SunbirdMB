using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using Sunbird.Core;
using SunbirdMB.Core;
using SunbirdMB.Framework;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;

namespace SunbirdMB
{
    public class MainGame : WpfGame
    {
        private IGraphicsDeviceService _graphicsDeviceManager;
        private IServiceContainer _services;
        private SpriteBatch _spriteBatch;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;
        public Color renderColor = Color.CornflowerBlue;

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
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;
        public Camera Camera { get; set; }
        internal WpfKeyboard Keyboard { get { return _keyboard; } }
        internal WpfMouse Mouse{ get { return _mouse; } }

        public bool cleanLoad = false;

        //public event EventHandler Loaded;

        protected override void Initialize()
        {
            // must be initialized. required by Content loading and rendering (will add itself to the Services)
            // note that MonoGame requires this to be initialized in the constructor, while WpfInterop requires it to
            // be called inside Initialize (before base.Initialize())
            _graphicsDeviceManager = new WpfGraphicsDeviceService(this);

            // wpf and keyboard need reference to the host control in order to receive input
            // this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);

            _services = new ServiceContainer();
            _services.AddService(typeof(IGraphicsDeviceService), _graphicsDeviceManager);
            Content = new Microsoft.Xna.Framework.Content.ContentManager(_services);
            Content.RootDirectory = "Content";

            Serializer.ExtraTypes = new Type[]
            {
                typeof(Cube),
                typeof(Deco),
            };

            Camera = new Camera(this);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            if (cleanLoad == true)
            {
                AssetLibraries.RebuildLibraries(this);
            }
            else
            {
                AssetLibraries.ImportLibraries(this);
            }

            // must be called after the WpfGraphicsDeviceService instance was created
            base.Initialize();

            // content loading now possible

            CurrentState = new MapBuilder(this, GraphicsDevice, Content, "MapBuilderSave.xml");
        }

        public void OnExit()
        {
            CurrentState.OnExit();
            var cubeFactoryData = new CubeFactoryData();
            cubeFactoryData.SyncIn();
            cubeFactoryData.Serialize();

            var decoFactoryData = new DecoFactoryData();
            decoFactoryData.SyncIn();
            decoFactoryData.Serialize();
        }

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
            // get and cache the wpf rendertarget (there is always a default rendertarget)
            var wpfRenderTarget = (RenderTarget2D)GraphicsDevice.GetRenderTargets()[0].RenderTarget;
            GraphicsDevice.SetRenderTarget(wpfRenderTarget);
            string colorcode = "#1e1e1e";
            int argb = Int32.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
            System.Drawing.Color clr = System.Drawing.Color.FromArgb(argb);
            GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color(clr.R, clr.G, clr.B));
            _spriteBatch.Begin(transformMatrix: Camera.CurrentTransform, samplerState: SamplerState);
            CurrentState.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();
        }
    }
}
