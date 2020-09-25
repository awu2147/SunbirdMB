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
    public class Keyboard
    {
        private WpfKeyboard WpfKeyboard;

        public Keyboard() { }

        public Keyboard(WpfGame game)
        {
            WpfKeyboard = new WpfKeyboard(game);
        }

        public KeyboardState GetState()
        {
            return WpfKeyboard == null ? Microsoft.Xna.Framework.Input.Keyboard.GetState() : WpfKeyboard.GetState();
        }
    }
}
