using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.GUI
{
    /// <summary>
    /// A simple game timer supporting both countdown and count-up modes.
    /// </summary>
    internal class GameTimer
    {
        private double m_durationSeconds;    // configured duration for countdown; 0 => count-up mode
        private double m_elapsedSeconds;     // elapsed time since Start
        private double m_secondsLeft;        // remaining seconds for countdown (kept in sync)
        private bool m_isCountdown;
        private bool m_isRunning;

        
        private SpriteFont m_font;
        private Color m_colour = Color.White;
        private float m_topOffset = 10f;


        /// Constructor for count-up timer with default draw style.
        public GameTimer() : this(0.0, null, Color.White, 10f) { }

        /// <summary>
        /// Constructor for countdown timer with specified duration in seconds.
        /// </summary>
        public GameTimer(double durationSeconds, SpriteFont font = null, Color? colour = null, float topOffset = 10f)
        {
            m_durationSeconds = Math.Max(0.0, durationSeconds);
            m_isCountdown = m_durationSeconds > 0.0;
            m_font = font;
            if (colour.HasValue) m_colour = colour.Value;
            m_topOffset = topOffset;
            Reset();
        }

        // Start / Stop
        public void Start() => m_isRunning = true;
        public void Stop() => m_isRunning = false;

        /// <summary>
        // Reset (preserves configured duration)
        /// </summary>
        public void Reset()
        {
            m_elapsedSeconds = 0.0;
            m_secondsLeft = m_isCountdown ? m_durationSeconds : 0.0;
            m_isRunning = false;
            IsFinished = m_isCountdown ? m_secondsLeft <= 0.0 : false;
        }

        /// <summary>
        // Reset and change duration (positive => countdown)
        /// </summary>
        public void Reset(double durationSeconds)
        {
            m_durationSeconds = Math.Max(0.0, durationSeconds);
            m_isCountdown = m_durationSeconds > 0.0;
            Reset();
        }

        /// <summary>
        /// Advance the timer using a <see cref="GameTime"/> instance.
        /// Delegates to the <see cref="Update(float)"/> overload using the frame's elapsed seconds.
        /// If the timer is not running or already finished this call has no effect.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            Update(gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// Advance the timer by a delta time in seconds.
        /// Delegates to the core <see cref="Update(double)"/> implementation.
        /// If the timer is not running or already finished this call has no effect.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            Update((double)deltaSeconds);
        }

        /// <summary>
        /// Core update implementation.
        /// - Increments elapsed time when the timer is running.
        /// - When in countdown mode, decreases the remaining time and sets <see cref="IsFinished"/>
        ///   and stops the timer when the remaining time reaches zero.
        /// - In count-up mode only the elapsed time is tracked.
        /// </summary>
        private void Update(double deltaSeconds)
        {
            if (!m_isRunning || IsFinished) return;

            m_elapsedSeconds += deltaSeconds;

            if (m_isCountdown)
            {
                m_secondsLeft -= deltaSeconds;
                if (m_secondsLeft <= 0.0)
                {
                    m_secondsLeft = 0.0;
                    IsFinished = true;
                    m_isRunning = false;
                }
            }
        }

        // Query
        public bool IsFinished { get; private set; } = false;

        // For compatibility with existing code expecting TimeSpan total time (count-up elapsed)
        public TimeSpan GetTotalTime() => TimeSpan.FromSeconds(Math.Max(0.0, m_elapsedSeconds));
        public TimeSpan GetElapsedTime() => GetTotalTime();

        // Remaining seconds (countdown) or elapsed (count-up)
        public double SecondsLeft => m_isCountdown ? m_secondsLeft : m_elapsedSeconds;

        /// <summary>
        // Formatted MM:SS for display. For countdown shows remaining, otherwise elapsed.
        // used by Draw().
        /// </summary>
        public string GetTimeString()
        {
            int seconds = (int)Math.Ceiling(SecondsLeft);
            if (seconds < 0) seconds = 0;
            int mins = seconds / 60;
            int secs = seconds % 60;
            return string.Format("{0:00}:{1:00}", mins, secs);
        }

        /// <summary>
        // Draw using provided SpriteBatch. If no font supplied, load game's default handwritingfont.
        // Assumes spriteBatch.Begin() has already been called by caller.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) return;

            // choose font
            var font = m_font ?? Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");

            string timeText = GetTimeString();
            Vector2 size = font.MeasureString(timeText);
            int width = spriteBatch.GraphicsDevice.Viewport.Width;
            float x = (width - size.X) / 2f;
            float y = m_topOffset;

            spriteBatch.DrawString(font, timeText, new Vector2(x, y), m_colour);
        }

        /// <summary>
        // Optional: allow setting draw style at runtime
        /// </summary>
        public void SetDrawStyle(SpriteFont font, Color colour, float topOffset = 10f)
        {
            m_font = font;
            m_colour = colour;
            m_topOffset = topOffset;
        }
    }
}