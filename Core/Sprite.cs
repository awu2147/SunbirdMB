using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using SunbirdMB.Framework;
using SunbirdMB.Interfaces;

namespace SunbirdMB.Core
{
    public enum Alignment
    {
        TopLeft,
        TopRight,
        Center,
        BottomLeft,
        BottomRight
    }

    public class Sprite
    {
        public static readonly XmlSerializer SpriteSerializer = Serializer.CreateNew(typeof(Sprite));

        /// <summary>
        /// The game that this sprite belongs to.
        /// </summary>
        [XmlIgnore]
        public IMainGame MainGame { get; set; }
        public Animator Animator { get; set; }
        public float Alpha { get; set; } = 1f;
        public Vector2 Position { get; set; }
        public Vector2 PositionOffset { get; set; }
        public Coord2D Coords { get; set; }
        public int Altitude { get; set; }
        public int DrawPriority { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSolid { get; set; }

        [XmlIgnore]
        public Texture2D Light;
        public string LightPath { get; set; }

        [XmlIgnore]
        public Texture2D Shadow;
        public string ShadowPath { get; set; }

        [XmlIgnore]
        public Texture2D AntiShadow;
        public string AntiShadowPath { get; set; }

        [XmlIgnore]
        public Texture2D SelfShadow;

        public event EventHandler Clicked;

        public Sprite() { }

        public Sprite(IMainGame mainGame, SpriteSheet spriteSheet) 
            : this(mainGame, spriteSheet, Vector2.Zero) { }

        public Sprite(IMainGame mainGame, SpriteSheet spriteSheet, Vector2 position) 
            : this(mainGame, spriteSheet, position, Alignment.TopLeft, null) { }

        public Sprite(IMainGame mainGame, SpriteSheet spriteSheet, Vector2 position, Alignment alignment) 
            : this(mainGame, spriteSheet, position, alignment, null) { }

        public Sprite(IMainGame mainGame, SpriteSheet spriteSheet, AnimArgs animArgs) 
            : this(mainGame, spriteSheet, Vector2.Zero, Alignment.TopLeft, animArgs) { }

        public Sprite(IMainGame mainGame, SpriteSheet spriteSheet, Vector2 position, Alignment alignment, AnimArgs animArgs)
        {
            MainGame = mainGame;
            // An animator is always required for the sprite to be drawn. The default constructor assumes the given spritesheet to be a single frame image.
            Animator = new Animator(this, spriteSheet);
            if (animArgs != null)
            {
                Animator.Reconfigure(animArgs);
            }

            if (GraphicsHelper.AntiShadowLibrary.ContainsKey(Animator.SpriteSheet.TexturePath))
            {
                AntiShadow = GraphicsHelper.AntiShadowLibrary[Animator.SpriteSheet.TexturePath];
            }
            else
            {
                AntiShadow = GraphicsHelper.GetAntiShadow(mainGame, Animator.SpriteSheet.Texture);
                GraphicsHelper.AntiShadowLibrary.Add(Animator.SpriteSheet.TexturePath, AntiShadow);
            }

            if (GraphicsHelper.SelfShadowLibrary.ContainsKey(Animator.SpriteSheet.TexturePath))
            {
                SelfShadow = GraphicsHelper.SelfShadowLibrary[Animator.SpriteSheet.TexturePath];
            }
            else
            {
                SelfShadow = GraphicsHelper.GetSelfShadow(mainGame, Animator.SpriteSheet.Texture);
                GraphicsHelper.SelfShadowLibrary.Add(Animator.SpriteSheet.TexturePath, SelfShadow);
            }

            if (alignment == Alignment.TopLeft)
            {
                Position = position;
            }
            else if (alignment == Alignment.TopRight)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width, 0);
            }
            else if (alignment == Alignment.Center)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width / 2, -spriteSheet.Texture.Height / 2);
            }
            else if (alignment == Alignment.BottomLeft)
            {
                Position = position + new Vector2(0, -spriteSheet.Texture.Height);
            }
            else if (alignment == Alignment.BottomRight)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width, -spriteSheet.Texture.Height);
            }
        }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public virtual void LoadContent(IMainGame mainGame)
        {
            MainGame = mainGame;
            if (Animator != null)
            {
                Animator.LoadContent(mainGame);
                Animator.Sprite = this;
            }
        }

        public virtual void OnClicked()
        {
            EventHandler handler = Clicked;
            handler?.Invoke(this, null);
        }       

        public virtual void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {
                Animator.Draw(gameTime, spriteBatch, Alpha);
            }
        }
    }
}

