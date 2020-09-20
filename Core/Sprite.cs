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

        [XmlIgnore]
        public MainGame MainGame { get; set; }
        public Animator Animator { get; set; }
        public float Alpha { get; set; } = 1f;
        public Vector2 Position { get; set; }
        public Vector2 PositionOffset { get; set; }
        public Coord Coords { get; set; }
        public int Altitude { get; set; }
        public int DrawPriority { get; set; }
        public bool IsHidden { get; set; }

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

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet) 
            : this(mainGame, spriteSheet, Vector2.Zero) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position) 
            : this(mainGame, spriteSheet, position, Alignment.TopLeft, null) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, Alignment alignment) 
            : this(mainGame, spriteSheet, position, alignment, null) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, AnimArgs animArgs) 
            : this(mainGame, spriteSheet, Vector2.Zero, Alignment.TopLeft, animArgs) { }

        public Sprite(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, Alignment alignment, AnimArgs animArgs)
        {
            MainGame = mainGame;
            Animator = new Animator(this, spriteSheet);
            if (animArgs != null)
            {
                ReconfigureAnimator(animArgs);
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
        public virtual void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            MainGame = mainGame;
            if (Animator != null)
            {
                Animator.LoadContent(content);
                Animator.Owner = this;
                if (this is Deco)
                {
                    var path = Animator.SpriteSheet.TexturePath;
                    if (DecoFactory.DecoMetaDataLibrary.ContainsKey(path))
                    {
                        SelfShadow = DecoFactory.DecoMetaDataLibrary[path].SelfShadow;
                        AntiShadow = DecoFactory.DecoMetaDataLibrary[path].AntiShadow;
                    }
                }
                // Currently, Cube (anti)shadow textures do not need to be dynamically generated. This may change if pixel perfect click detection required.
                else
                {
                    // This is very slow, assign textures through libraries where possible.
                    // Also results in memory leak if called after instantiation (use SafeLoadContent instead).

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
                    Debug.Print(GraphicsHelper.AntiShadowLibrary.Count().ToString());
                    Debug.Print(GraphicsHelper.SelfShadowLibrary.Count().ToString());
                }
            }
            // These are typically null if dynamic generation of texture occurs at some point in time i.e. here or library creation.
            if (ShadowPath != null) { Shadow = content.Load<Texture2D>(ShadowPath); }
            if (AntiShadowPath != null) { AntiShadow = content.Load<Texture2D>(AntiShadowPath); }
            if (LightPath != null) { Light = content.Load<Texture2D>(LightPath); }
        }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This is safe to call during runtime.
        /// </summary>
        public virtual void SafeLoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            MainGame = mainGame;
            if (Animator != null)
            {
                Animator.LoadContent(content);
                Animator.Owner = this;
            }
            if (ShadowPath != null) { Shadow = content.Load<Texture2D>(ShadowPath); }
            if (AntiShadowPath != null) { AntiShadow = content.Load<Texture2D>(AntiShadowPath); }
            if (LightPath != null) { Light = content.Load<Texture2D>(LightPath); }
        }

        public virtual void OnClicked()
        {
            EventHandler handler = Clicked;
            handler?.Invoke(this, null);
        }

        /// <summary>
        /// Replace the SpriteSheet of the default Sprite Animator. This is usually followed by a ReconfigureAnimator method call.
        /// </summary>
        /// <param name="newSheet"></param>
        public void ReplaceSpriteSheet(SpriteSheet newSheet)
        {
            ReplaceSpriteSheet(newSheet, Animator);
        }

        /// <summary>
        /// Replace the SpriteSheet of a specified Animator. This is usually followed by a ReconfigureAnimator method call.
        /// </summary>
        /// <param name="newSheet"></param>
        /// <param name="animator"></param>
        public void ReplaceSpriteSheet(SpriteSheet newSheet, Animator animator)
        {
            animator.SpriteSheet = newSheet;
        }

        /// <summary>
        /// Reconfigure the default Sprite Animator using Current frame = Start frame.
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            ReconfigureAnimator(startFrame, startFrame, frameCount, frameSpeed, animState, Animator);
        }

        /// <summary>
        /// Reconfigure the default Sprite Animator. 
        /// </summary>
        public void ReconfigureAnimator(AnimArgs args)
        {
            ReconfigureAnimator(args.StartFrame, args.CurrentFrame, args.FramesInLoop, args.FrameSpeed, args.AnimState);
        }

        /// <summary>
        /// Reconfigure the default Sprite Animator. 
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            ReconfigureAnimator(startFrame, currentFrame, frameCount, frameSpeed, animState, Animator);
        }

        /// <summary>
        /// Reconfigure the specified Animator using Current frame = Start frame.
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int frameCount, float frameSpeed, AnimationState animState, Animator animator)
        {
            ReconfigureAnimator(startFrame, startFrame, frameCount, frameSpeed, animState, animator);
        }

        /// <summary>
        /// Reconfigure the specified Animator.
        /// </summary>
        public void ReconfigureAnimator(int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState, Animator animator)
        {
            animator.StartFrame = startFrame;
            animator.CurrentFrame = currentFrame;
            animator.FrameCounter = currentFrame - startFrame;
            animator.FramesInLoop = frameCount;
            animator.FrameSpeed = frameSpeed;
            animator.AnimState = animState;
            animator.Timer.Reset();
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

