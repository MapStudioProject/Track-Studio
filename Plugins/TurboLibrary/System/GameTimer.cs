using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TurboLibrary
{
    /// <summary>
    /// Represents a timer to control the game calculations seperate from rendering.
    /// </summary>
    public class GameTimer : IDisposable
    {
        /// <summary>
        /// Detemines to pause or play the timer.
        /// </summary>
        public GameState TimerState = GameState.Pause;

        /// <summary>
        /// Determines if the current timer is being played.
        /// </summary>
        public bool IsPlaying => TimerState == GameState.Playing;

        /// <summary>
        /// A callback to run before the frame update event.
        /// </summary>
        public EventHandler OnFrameUpdate;

        private Stopwatch stopWatch;
        private System.Timers.Timer animationTimer;

        private float FrameRate = 60.0f;
        private float CurrentFrame;

        private int timing = 0;
        private bool disposed = false;

        public GameTimer()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();

            animationTimer = new System.Timers.Timer()
            {
                Interval = (int)(1000.0f / 60.0f),
            };
            animationTimer.Elapsed += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e) {
            if (disposed) return;

            CurrentFrame += 1.0f;
            OnFrameAdvanced();
        }

        /// <summary>
        /// Updates the current frame rate of the timer.
        /// </summary>
        public void UpdateFramerate(float rate) {
            FrameRate = rate;
            animationTimer.Interval = (int)(1000.0f / FrameRate);
        }

        /// <summary>
        /// Activates the timer to start playing.
        /// </summary>
        public void Play()
        {
            animationTimer.Start();
            TimerState = GameState.Playing;
        }

        /// <summary>
        /// Pauses the timer if it is currently playing.
        /// </summary>
        public void Pause()
        {
            animationTimer.Stop();
            TimerState = GameState.Pause;
        }

        private void OnFrameAdvanced()
        {
            timing += (int)stopWatch.ElapsedMilliseconds;
            stopWatch.Reset();
            stopWatch.Start();

            //Update by interval
            if (timing > 16)
            {
                timing = timing % 16;

                OnFrameUpdate?.Invoke(this, EventArgs.Empty);
                OnUpdateFrame(CurrentFrame);
            }
        }

        /// <summary>
        /// Method for updating the frame during the timer playback.
        /// </summary>
        /// <param name="frame"></param>
        protected virtual void OnUpdateFrame(float frame)
        {

        }

        /// <summary>
        /// Disposes the timer.
        /// </summary>
        public void Dispose()
        {
            animationTimer.Stop();
            animationTimer.Dispose();

            disposed = true;
            TimerState = GameState.Pause;
            stopWatch.Stop();
        }

        public enum GameState
        {
            Playing,
            Pause,
        }
    }
}
