using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;

namespace SunbirdMB.Core
{
    public class DecoReference : Sprite, IWorldObject
    {
        public Deco Reference { get; set; }
        public Coord3D ReferenceCoord { get; set; }

        public DecoReference() { }

        public DecoReference(Deco reference, Coord3D referenceCoord)
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
            if (Reference.Dimensions.X == 3 && Reference.Dimensions.Y == 3)
            {
                var totalSlices = 3;
                //        x
                //     x     x
                //  x     @     x
                //     x     x
                //        o
                // Bottom layer, draw the o.
                if (Coords.X - ReferenceCoord.X == -(Coords.Y - ReferenceCoord.Y) && ReferenceCoord.Z == Altitude && Coords.X - ReferenceCoord.X > 0)
                {
                    Reference.Draw3x3Patch(gameTime, spriteBatch, totalSlices - 1, totalSlices);
                }
            }
        }
    }
}
