using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using SunbirdMB.Framework;

namespace SunbirdMB.Core
{
    public class AnimArgs
    {
        public int StartFrame { get; set; } = 1;
        public int CurrentFrame { get; set; } = 1;
        public int FramesInLoop { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;

        public AnimArgs() { }

        public AnimArgs(int startFrame) : this(startFrame, startFrame, 1, 0.133f, AnimationState.None) { }

        public AnimArgs(int startFrame, int framesInLoop, float frameSpeed, AnimationState animState) : this(startFrame, startFrame, framesInLoop, frameSpeed, animState) { }

        public AnimArgs(int startFrame, int currentFrame, int framesInLoop, float frameSpeed, AnimationState animState)
        {
            StartFrame = startFrame;
            CurrentFrame = currentFrame;
            FramesInLoop = framesInLoop;
            FrameSpeed = frameSpeed;
            AnimState = animState;
        }
    }

    public class Animator
    {
        /// <summary>
        /// The sprite that owns this animator.
        /// </summary>
        [XmlIgnore]
        public Sprite Sprite { get; set; }

        [XmlIgnore]
        public Dictionary<int, Point> PositionMap { get { return SpriteSheet.PositionMap; } }

        public SpriteSheet SpriteSheet { get; set; }
        public Vector2 Position { get { return Sprite.Position + Sprite.PositionOffset; } }

        public int StartFrame { get; set; } = 1;
        public int CurrentFrame { get; set; } = 1;
        public int FramesInLoop { get; set; } = 1;
        public int FrameCounter { get; set; }
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public Timer Timer { get; set; } = new Timer();


        private Animator() { }

        public Animator(Sprite owner, SpriteSheet spriteSheet) 
            : this(owner, spriteSheet, 1, 1, 1, 0.133f, AnimationState.None) { }

        public Animator(Sprite owner, SpriteSheet spriteSheet, int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            Sprite = owner;
            SpriteSheet = spriteSheet;
            StartFrame = startFrame;
            CurrentFrame = currentFrame;
            FramesInLoop = frameCount;
            FrameSpeed = frameSpeed;
            AnimState = animState;
            Timer.OnCompleted = () =>
            {
                CurrentFrame++;
                FrameCounter++;
            };
        }

        public virtual void LoadContent(ContentManager content)
        {
            SpriteSheet.LoadContent(content);
            Timer.OnCompleted = () =>
            {
                CurrentFrame++;
                FrameCounter++;
            };
        }

        /// <summary>
        /// Gets the rectangular area occupied by the drawn frame with respect to its spritesheet.
        /// </summary>
        /// <returns></returns>
        public Rectangle SheetViewArea()
        {
            return new Rectangle(PositionMap[CurrentFrame - 1].X, PositionMap[CurrentFrame - 1].Y, SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
        }

        /// <summary>
        /// Gets the rectangular area occupied by the drawn frame with respect to the World.
        /// </summary>
        /// <returns></returns>
        public Rectangle WorldArea()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
        }

        /// <summary>
        /// Replace the SpriteSheet. This is usually followed by a ReconfigureAnimator method call.
        /// </summary>
        /// <param name="newSheet"></param>
        public void ReplaceSpriteSheet(SpriteSheet newSheet)
        {
            SpriteSheet = newSheet;
        }

        /// <summary>
        /// Reconfigure the Animator. 
        /// </summary>
        public void Reconfigure(AnimArgs args)
        {
            Reconfigure(args.StartFrame, args.CurrentFrame, args.FramesInLoop, args.FrameSpeed, args.AnimState);
        }

        /// <summary>
        /// Reconfigure the Animator using Current frame = Start frame.
        /// </summary>
        public void Reconfigure(int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            Reconfigure(startFrame, startFrame, frameCount, frameSpeed, animState);
        }

        /// <summary>
        /// Reconfigure the Animator.
        /// </summary>
        public void Reconfigure(int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            StartFrame = startFrame;
            CurrentFrame = currentFrame;
            FrameCounter = currentFrame - startFrame;
            FramesInLoop = frameCount;
            FrameSpeed = frameSpeed;
            AnimState = animState;
            Timer.Reset();
        }

        public void Update(GameTime gameTime)
        {
            if (AnimState == AnimationState.Loop)
            {
                Timer.WaitForSeconds(gameTime, FrameSpeed);
                if (FrameCounter >= FramesInLoop)
                {
                    CurrentFrame = StartFrame;
                    FrameCounter = 0;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float alpha)
        {
            spriteBatch.Draw(SpriteSheet.Texture, Position,
            new Rectangle(PositionMap[CurrentFrame - 1].X, PositionMap[CurrentFrame - 1].Y,
            SpriteSheet.FrameWidth, SpriteSheet.FrameHeight), Color.White * alpha);
        }
    }
}

