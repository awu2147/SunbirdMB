using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Interfaces
{
    public interface IMainGame
    {
        GraphicsDevice GraphicsDevice { get; }
        ContentManager Content { get; set; }
        Mouse Mouse { get; set; }
        Keyboard Keyboard { get; set; }        
        bool IsActive { get; }
    }
}
