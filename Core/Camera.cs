using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public Vector2 DragTargetPosition { get; set; }
        private Point DragPositionChange { get; set; }
        private Point Anchor { get; set; }

        // TODO: Push
        public Matrix PushTransform { get; set; } = Matrix.Identity;
        private Direction PushDirection { get; set; }
        private float Counter { get; set; } = 3;

        private SunbirdMBGame MainGame { get; set; }

        /// <summary>
        /// Back buffer width.
        /// </summary>
        private int Width { get { return MainGame.BackBufferWidth; } }

        /// <summary>
        /// Back buffer height.
        /// </summary>
        private int Height { get { return MainGame.BackBufferHeight; } }


        public Camera(SunbirdMBGame sender)
        {
            MainGame = sender;
        }

        public void Update(SunbirdMBGame mainGame)
        {
            if (CurrentMode == CameraMode.Drag)
            {
                CurrentTransform = DragTransform;
            }
            Drag(MainGame);
        }

        public void Drag(SunbirdMBGame mainGame)
        {
            //Wrap this in else block if toggling.
            if (Peripherals.MiddleButtonPressed() && MainGame.IsActive == true)
            {
                CurrentMode = CameraMode.Drag;
                if (Peripherals.MiddleButtonTapped())
                {
                    Peripherals.MiddleButtonReleased += Peripherals_MiddleButtonReleased;
                    Anchor = Peripherals.GetMouseWindowPosition(mainGame);
                }
                var currentPosition = Peripherals.GetMouseWindowPosition(mainGame);
                DragPositionChange = (currentPosition - Anchor) * new Point(World.Scale, World.Scale) / new Point(World.Zoom, World.Zoom);
                DragTransform = CreateDragTransform();
            }
            //Wrap this in else block if toggling.
        }

        private void Peripherals_MiddleButtonReleased(object sender, EventArgs e)
        {
            DragTargetPosition -= DragPositionChange.ToVector2();
            DragPositionChange = Point.Zero;
            Peripherals.MiddleButtonReleased -= Peripherals_MiddleButtonReleased;
        }

        public void RecreateDragTransform()
        {
            DragTransform = CreateDragTransform();
        }

        public Matrix CreateDragTransform()
        {
            var xChange = (Width / 2f) / World.ZoomRatio - DragTargetPosition.X + DragPositionChange.X;
            var yChange = (Height / 2f) / World.ZoomRatio - DragTargetPosition.Y + DragPositionChange.Y;
            return Matrix.CreateTranslation((float)Math.Floor(xChange), (float)Math.Floor(yChange), 0) * Matrix.CreateScale(World.ZoomRatio);
        }
       
    }
}
