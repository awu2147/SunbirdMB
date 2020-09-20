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

namespace SunbirdMB.Core
{
    public enum AnimationState
    {
        None,
        Once,
        Loop
    }

    public class AnimArgs
    {
        public int StartFrame { get; set; }
        public int CurrentFrame { get; set; }
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

    [Serializable]
    public class Animator
    {
        [XmlIgnore]
        public Sprite Owner { get; set; }

        [XmlIgnore]
        public Dictionary<int, Point> PositionMap { get { return SpriteSheet.PositionMap; } }

        public SpriteSheet SpriteSheet { get; set; }
        public Vector2 Position { get { return Owner.Position + Owner.PositionOffset; } }

        public int StartFrame { get; set; }
        public int CurrentFrame { get; set; }
        public int FramesInLoop { get; set; } = 1;
        public int FrameCounter { get; set; }
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public Timer Timer { get; set; } = new Timer();


        private Animator() { }

        public Animator(Sprite owner, SpriteSheet spriteSheet) 
            : this(owner, spriteSheet, 0, 0, 1, 0.133f, AnimationState.None) { }

        public Animator(Sprite owner, SpriteSheet spriteSheet, int startFrame, int currentFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            Owner = owner;
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
            return new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y, SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
        }

        /// <summary>
        /// Gets the rectangular area occupied by the drawn frame with respect to the World.
        /// </summary>
        /// <returns></returns>
        public Rectangle WorldArea()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
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
            new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y,
            SpriteSheet.FrameWidth, SpriteSheet.FrameHeight), Color.White * alpha);
        }
    }
}

