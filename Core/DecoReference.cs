using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunbirdMB.Core
{
    public class DecoReference : Sprite
    {
        public Sprite Reference { get; set; }
        public Coord3D ReferenceCoord { get; set; }

        public DecoReference() { }

        public DecoReference(Sprite reference, Coord3D referenceCoord)
        {
            Reference = reference;
            ReferenceCoord = referenceCoord;
        }

        public override void Update(GameTime gameTime)
        {
            // Do nothing.
        }

        public override void LoadContent(IMainGame mainGame)
        {
            // Do nothing.
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Do nothing.
            // Reference.Draw(gameTime, spriteBatch);
        }
    }
}
