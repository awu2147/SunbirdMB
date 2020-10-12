using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;

namespace SunbirdMB.Core
{
    public class GhostMarker : Sprite
    {
        public Sprite Image { get; set; }
        public bool DrawDefaultMarker { get; set; }

        private GhostMarker() { }

        public GhostMarker(IMainGame mainGame, SpriteSheet spriteSheet) : base(mainGame, spriteSheet) 
        { 
            Alpha = 0.3f;
        }

        public void MorphImage(Sprite image)
        {
            // Morph the ghost marker into the given sprite by serialization.
            Image = Serializer.CloneBySerialization(image, SpriteSerializer);
            // Apply ghost marker properties to the deserialized image.
            Image.IsHidden = IsHidden;
            Image.Position = Position;
            // Load the image content.
            Image.LoadContent(MainGame);
        }

        public void MorphCurrentDeco()
        {
            MorphImage(DecoFactory.CreateCurrentDeco(Coord.Zero, Coord.Zero, 0));
        }

        public void MorphCurrentCube()
        {
            MorphImage(CubeFactory.CreateCurrentCube(Coord.Zero, Coord.Zero, 0));
        }

        public override void LoadContent(IMainGame mainGame)
        {
            base.LoadContent(mainGame);
            Image.LoadContent(mainGame);
        }

        public override void Update(GameTime gameTime)
        {
            

            base.Update(gameTime); // This is not really needed since animator position updated through getter, and TopFaceMarker is not animated.
            Image.Update(gameTime);
            Image.IsHidden = IsHidden;
            Image.Position = Position;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (DrawDefaultMarker == true)
            {
                base.Draw(gameTime, spriteBatch);
            }
            else
            {
                Image.Alpha = 0.3f;
                Image.Draw(gameTime, spriteBatch);
            }
        }

    }
}
