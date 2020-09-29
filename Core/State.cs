using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Runtime.Serialization;

namespace SunbirdMB.Core
{
    /// <summary>
    /// A MainGame State; MainGame can only be in one State at a time. 
    /// </summary>
    public abstract class State
    {
        protected SunbirdMBGame MainGame { get; set; }

        public event EventHandler StateChanged;

        /// <summary>
        /// Only used for serialization.
        /// </summary>
        public State()
        {

        }

        public State(SunbirdMBGame mainGame)
        {
            MainGame = mainGame;
        }

        public void OnStateChanged()
        {
            EventHandler handler = StateChanged;
            handler?.Invoke(this, null);
        }

        public abstract void SaveAndSerialize();

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        public abstract void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch);

    }
}

