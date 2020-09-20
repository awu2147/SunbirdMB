using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sunbird.Core;
using SunbirdMB.Framework;

namespace SunbirdMB.Core
{
    public enum CameraMode
    {
        Drag = 0,
    }

    public class Camera
    {
        public Matrix CurrentTransform { get; set; } = Matrix.Identity;
        public CameraMode CurrentMode { get; set; }

        // Drag
        public Matrix DragTransform { get; set; } = Matrix.Identity;
        private Vector2 DragTargetPosition { get; set; }
        private Point DragPositionChange { get; set; }
        private Point Anchor { get; set; }

        // TODO: Push
        public Matrix PushTransform { get; set; } = Matrix.Identity;
        private Direction PushDirection { get; set; }
        private float Counter { get; set; } = 3;

        private MainGame MainGame { get; set; }
        private int Width { get { return MainGame.BackBufferWidth; } }
        private int Height { get { return MainGame.BackBufferHeight; } }


        public Camera(MainGame sender)
        {
            MainGame = sender;
            DragTransform = CreateDragTransform();
        }

        public void Update(MainGame mainGame)
        {
            if (CurrentMode == CameraMode.Drag)
            {
                CurrentTransform = DragTransform;
            }
            Drag(MainGame);
        }

        public void Drag(MainGame mainGame)
        {
            //Wrap this in else block if toggling.
            if (Peripherals.MiddleButtonPressed() && MainGame.IsActive == true)
            {
                CurrentMode = CameraMode.Drag;
                MainGame.SamplerState = SamplerState.AnisotropicClamp;
                if (Peripherals.MiddleButtonTapped())
                {
                    Peripherals.MiddleButtonReleased += peripherals_MiddleButtonReleased;
                    Anchor = Peripherals.GetMouseWindowPosition(mainGame);
                }
                var currentPosition = Peripherals.GetMouseWindowPosition(mainGame);
                DragPositionChange = (currentPosition - Anchor) * new Point(World.Scale, World.Scale) / new Point(World.Zoom, World.Zoom);
                DragTransform = CreateDragTransform();
            }
            else
            {
                MainGame.SamplerState = SamplerState.PointClamp;
            }
            //Wrap this in else block if toggling.
        }

        private void peripherals_MiddleButtonReleased(object sender, EventArgs e)
        {
            DragTargetPosition -= DragPositionChange.ToVector2();
            DragPositionChange = Point.Zero;
            Peripherals.MiddleButtonReleased -= peripherals_MiddleButtonReleased;
        }

        public Matrix CreateDragTransform()
        {
            return Matrix.CreateTranslation((Width / 2f) / World.ZoomRatio - DragTargetPosition.X + DragPositionChange.X,
                                            (Height / 2f) / World.ZoomRatio - DragTargetPosition.Y + DragPositionChange.Y, 0) * Matrix.CreateScale(World.ZoomRatio);
        }
       
    }
}
