using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;

namespace Pets_Enhanced_Mod.Multiplayer
{
    public class LatencyCompensatedPredictor
    {
        public LatencyCompensatedPredictor(Vector2 _targetPosition, Vector2 _currentVelocity)
        {
            targetPosition = _targetPosition;
            CurrentPosition = targetPosition;
            originalPosition = targetPosition;
            lastRTT = Game1.currentGameTime.TotalGameTime.TotalSeconds;
        }
        public Vector2 originalPosition;
        private Vector2 targetPosition;

        public Vector2 CurrentPosition;

        private float lerpSpeed = 15.0f;
        private float snapThreshold = 500.0f;

        public float SmoothRTT { get; private set; } = 0.1f; // Default 100ms
        private const float SmoothingFactor = 0.1f;
        private double lastRTT;

        public void OnReceiveServerUpdate(Vector2 serverPos, Vector2 serverVel)
        {
            originalPosition = serverPos;
            double currentTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
            float measuredRTT = (float)(currentTime - lastRTT);

            SmoothRTT = (SmoothRTT * (1f - SmoothingFactor)) + (measuredRTT * SmoothingFactor);
            lastRTT = currentTime;

            targetPosition = serverPos + (serverVel * SmoothRTT);

            if (Vector2.Distance(CurrentPosition, targetPosition) > snapThreshold)
            {
                CurrentPosition = targetPosition;
            }
            
        }

        public int lastTick = 0;
        public void Update(Vector2 velocity)
        {
            if (!ModEntry.ShouldTimePass) { return; }

            if (lastTick != Game1.ticks)
            {
                lastTick = Game1.ticks;

                targetPosition += velocity;

                CurrentPosition = Vector2.Lerp(CurrentPosition, targetPosition, 0.5f);
            }
        }
    }
}
