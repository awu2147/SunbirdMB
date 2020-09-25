using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Framework
{
    public class Mouse
    {
        private WpfMouse WpfMouse;

        public Mouse() { }

        public Mouse(WpfGame wpfGame)
        {
            WpfMouse = new WpfMouse(wpfGame);
        }

        public MouseState GetState()
        {
            return WpfMouse == null ? Microsoft.Xna.Framework.Input.Mouse.GetState() : WpfMouse.GetState();
        }
    }
}
