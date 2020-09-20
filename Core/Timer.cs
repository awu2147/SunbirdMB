using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace SunbirdMB.Core
{
    public class Timer
    {
        public float TotalElapsedTime { get; set; } = 0;
        public bool IsDone { get; set; } = true;

        [XmlIgnore]
        public Action OnCompleted { get; set; } = () => { };

        public Timer()
        {

        }

        public void Reset()
        {
            TotalElapsedTime = 0;
            IsDone = true;
        }

        public void WaitForMilliseconds(GameTime gameTime, float waitTime)
        {
            if (IsDone)
            {
                TotalElapsedTime = 0;
                IsDone = false;
            }

            if (!IsDone && TotalElapsedTime < waitTime)
            {
                TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else if (!IsDone)
            {
                OnCompleted();
                IsDone = true;
            }
        }

        public void WaitForSeconds(GameTime gameTime, float waitTime)
        {
            if (IsDone)
            {
                TotalElapsedTime = 0;
                IsDone = false;
            }

            if (!IsDone && TotalElapsedTime < waitTime)
            {
                TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (!IsDone)
            {
                OnCompleted();
                IsDone = true;
            }
        }
    }
}

