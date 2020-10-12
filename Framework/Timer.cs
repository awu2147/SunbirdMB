using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace SunbirdMB.Core
{
    public class Timer
    {
        public float TotalElapsedTime { get; set; } = 0;
        public bool IsRunning { get; set; } = false;

        [XmlIgnore]
        public Action OnCompleted { get; set; } = () => { };

        public Timer()
        {

        }

        public void Reset()
        {
            TotalElapsedTime = 0;
            IsRunning = true;
        }

        public void WaitForMilliseconds(GameTime gameTime, float waitTime)
        {
            if (IsRunning && TotalElapsedTime < waitTime)
            {
                TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else if (IsRunning)
            {
                OnCompleted();
                IsRunning = false;
            }
        }

        public void WaitForSeconds(GameTime gameTime, float waitTime)
        {
            if (IsRunning && TotalElapsedTime < waitTime)
            {
                TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (IsRunning)
            {
                OnCompleted();
                IsRunning = false;
            }
        }
    }

    public class LoopTimer
    {
        public float TotalElapsedTime { get; set; } = 0;
        public bool IsRunning { get; set; } = false;

        [XmlIgnore]
        public Action OnCompleted { get; set; } = () => { };

        public LoopTimer()
        {

        }

        public void Reset()
        {
            TotalElapsedTime = 0;
            IsRunning = true;
        }

        public void WaitForMilliseconds(GameTime gameTime, float waitTime)
        {
            if (!IsRunning)
            {
                Reset();
            }

            if (IsRunning && TotalElapsedTime < waitTime)
            {
                TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else if (IsRunning)
            {
                OnCompleted();
                IsRunning = false;
            }
        }

        public void WaitForSeconds(GameTime gameTime, float waitTime)
        {
            if (!IsRunning)
            {
                Reset();
            }

            if (IsRunning && TotalElapsedTime < waitTime)
            {
                TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (IsRunning)
            {
                OnCompleted();
                IsRunning = false;
            }
        }
    }
}

