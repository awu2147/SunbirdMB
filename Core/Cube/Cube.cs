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
using Sunbird.Core;
using SunbirdMB.Interfaces;
using SunbirdMB.Framework;
using SunbirdMB.Gui;
using System.Reflection;

namespace SunbirdMB.Core
{
    /// <summary>
    /// A single unit building block.
    /// </summary>
    public class Cube : Sprite, IWorldObject
    {
        public Animator AnimatorTop { get; set; }
        public Animator AnimatorBase { get; set; }

        public Cube() { }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates.
        /// </summary>
        public override void LoadContent(IMainGame mainGame, ContentManager content)
        {
            AnimatorTop.LoadContent(content);
            AnimatorTop.Sprite = this;
            AnimatorBase.LoadContent(content);
            AnimatorBase.Sprite = this;
        }

        public override void Update(GameTime gameTime)
        {
            AnimatorTop.Update(gameTime);
            AnimatorBase.Update(gameTime);

            Debug.Assert(AnimatorTop.Position == Position);
            Debug.Assert(AnimatorBase.Position == Position);

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {
                AnimatorBase.Draw(gameTime, spriteBatch, Alpha);
                AnimatorTop.Draw(gameTime, spriteBatch, Alpha);
            }
        }

    }

}

