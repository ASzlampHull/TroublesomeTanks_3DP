using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tankontroller.GUI
{
    // Simple reusable countdown timer GUI.
    public class GameTimer
    {
        private double m_durationSeconds;    // configured duration for countdown; 0 => count-up mode
        private double m_elapsedSeconds;     // elapsed time since Start
        private double m_secondsLeft;        // remaining seconds for countdown (kept in sync)
        private bool m_isCountdown;
        private bool m_isRunning;

        private SpriteFont m_font;
        private Color m_colour = Color.White;
        private float m_topOffset = 10f;

        public GameTimer() : this(0.0, null, Color.White, 10f) { }

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

        // Reset (preserves configured duration)
        public void Reset()
        {
            m_elapsedSeconds = 0.0;
            m_secondsLeft = m_isCountdown ? m_durationSeconds : 0.0;
            m_isRunning = false;
            IsFinished = m_isCountdown ? m_secondsLeft <= 0.0 : false;
        }

        // Reset and change duration (positive => countdown)
        public void Reset(double durationSeconds)
        {
            m_durationSeconds = Math.Max(0.0, durationSeconds);
            m_isCountdown = m_durationSeconds > 0.0;
            Reset();
        }

        // Update overloads
        public void Update(GameTime gameTime)
        {
            Update(gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Update(float deltaSeconds)
        {
            Update((double)deltaSeconds);
        }

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

        // Formatted MM:SS for display. For countdown shows remaining, otherwise elapsed.
        public string GetTimeString()
        {
            int seconds = (int)Math.Ceiling(SecondsLeft);
            if (seconds < 0) seconds = 0;
            int mins = seconds / 60;
            int secs = seconds % 60;
            return string.Format("{0:00}:{1:00}", mins, secs);
        }

        // Draw using provided SpriteBatch. If no font supplied, load game's default handwritingfont.
        // Assumes spriteBatch.Begin() has already been called by caller.
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

        // Optional: allow setting draw style at runtime
        public void SetDrawStyle(SpriteFont font, Color colour, float topOffset = 10f)
        {
            m_font = font;
            m_colour = colour;
            m_topOffset = topOffset;
        }
    }
}