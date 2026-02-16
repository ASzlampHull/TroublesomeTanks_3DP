using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.World;
using Tankontroller.World.Particles;
using Tankontroller.Utilities;


namespace Tankontroller.World.Gameplay
{
    internal class DeathRing
    {
        private static readonly Texture2D m_CircleTexture = Tankontroller.Instance().CM().Load<Texture2D>("circle");

        // Config (loaded from DGS where present; safe defaults used)
        private readonly float m_activationTime;    // seconds before match end to activate
        private readonly float m_duration;          // how long shrink lasts
        private readonly float m_startRadius;       // computed from play area if not set in DGS
        private readonly float m_endRadius;         // final safe radius
        private readonly float m_damagePerSecond;   // DPS applied outside safe zone
        private readonly float m_graceSeconds;      // Seconds a tank can survive outside the ring before the first "tick" of damage.

        // State
        private readonly Vector2 m_center;
        private float m_elapsedSinceStart = 0f;
        private float m_currentRadius;
        private bool m_active = false;

        // Per-tank state
        private readonly Dictionary<Tank, float> m_outsideTime = new();
        private readonly Dictionary<Tank, float> m_damageAccumulator = new();

        public DeathRing(Rectangle playArea)
        {
            /// <summary>
            /// sets a float from DGS or returns default if not found or invalid
            /// </summary>
            float SafeFloat(string key, float def)
            {
                try { return DGS.Instance.GetFloat(key); } catch { return def; }
            }

            m_activationTime = SafeFloat("DEATH_RING_ACTIVATION_TIME", 45f);
            m_duration = SafeFloat("DEATH_RING_DURATION", Math.Max(1f, m_activationTime));
            m_damagePerSecond = SafeFloat("DEATH_RING_DPS", 10f);
            m_graceSeconds = SafeFloat("DEATH_RING_GRACE", 1f);

            m_center = new Vector2(playArea.X + playArea.Width / 2f, playArea.Y + playArea.Height / 2f);

            float defaultStart = MathF.Max(playArea.Width, playArea.Height) * 1.2f;
            float configuredStart = SafeFloat("DEATH_RING_START_RADIUS", defaultStart);
            m_startRadius = configuredStart > 0 ? configuredStart : defaultStart;

            float defaultEnd = MathF.Max(playArea.Width, playArea.Height) * 0.3f;
            float configuredEnd = SafeFloat("DEATH_RING_END_RADIUS", defaultEnd);
            m_endRadius = configuredStart > 0 ? configuredEnd : defaultEnd;

            m_currentRadius = m_startRadius;
        }

        /// <summary>
        /// Update ring state. Call every frame.
        /// - deltaSeconds: frame delta
        /// - remainingMatchSeconds: seconds left in match (countdown)
        /// - tanks: list of tanks (pass all active tanks)
        /// </summary>
        public void Update(float deltaSeconds, float remainingMatchSeconds, List<Tank> tanks)
        {
            if (!m_active)
            {
                if (remainingMatchSeconds <= m_activationTime)
                {
                    m_active = true;
                    // Align elapsed so the ring progress corresponds to time since activation
                    m_elapsedSinceStart = MathF.Max(0f, m_activationTime - remainingMatchSeconds);
                }
                else
                {
                    return;
                }
            }

            // Progress shrink (linear), clamped to not shrink below endRadius
            m_elapsedSinceStart += deltaSeconds;
            float t = (m_duration <= 0f) ? 1f : MathF.Min(1f, m_elapsedSinceStart / m_duration);
            m_currentRadius = MathHelper.Clamp(MathHelper.Lerp(m_startRadius, m_endRadius, t), m_endRadius, m_startRadius);


            // Damage application: continuous DPS after grace
            foreach (var tank in tanks)
            {
                if (tank == null) continue;
                Vector2 tankPos = tank.GetWorldPosition();
                float dist = Vector2.Distance(tankPos, m_center);
                bool outside = dist > m_currentRadius;

                if (outside)
                {
                    if (!m_outsideTime.ContainsKey(tank)) m_outsideTime[tank] = 0f;
                    m_outsideTime[tank] += deltaSeconds;

                    if (m_outsideTime[tank] >= m_graceSeconds)
                    {
                        if (!m_damageAccumulator.ContainsKey(tank)) m_damageAccumulator[tank] = 0f;
                        m_damageAccumulator[tank] += m_damagePerSecond * deltaSeconds;

                        float acc = m_damageAccumulator[tank];
                        int wholeHits = (int)MathF.Floor(acc);
                        if (wholeHits > 0)
                        {
                            for (int i = 0; i < wholeHits; i++)
                            {
                                tank.TakeDamage();
                            }
                            m_damageAccumulator[tank] = acc - wholeHits;
                        }
                    }
                }
                else
                {
                    // Reset when tank returns inside
                    if (m_outsideTime.ContainsKey(tank)) m_outsideTime.Remove(tank);
                    if (m_damageAccumulator.ContainsKey(tank)) m_damageAccumulator.Remove(tank);
                }
            }

            // Prune state for tanks that no longer exist in supplied list (avoid leaks)
            var toRemove = m_outsideTime.Keys.Where(k => !tanks.Contains(k)).ToList();
            foreach (var k in toRemove)
            {
                m_outsideTime.Remove(k);
                m_damageAccumulator.Remove(k);
            }
        }

        /// <summary>
        /// Draw ring visuals. Call between SpriteBatch.Begin/End.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!m_active) return;

            // color for ring (RGBA). adjust alpha to taste.
            Color ringColor = new Color(200, 30, 30, 200);

            // Warning ring color (yellow/orange, more transparent)
            Color warningColor = new Color(200, 30, 30, 200);

            // Draw a ring outline using DrawUtilities. This draws a pregenerated ring texture with transparent centre.
            DrawUtilities.DrawRing(spriteBatch, m_center, m_currentRadius, ringColor);

        }

        public bool IsActive() => m_active;
        public float CurrentRadius => m_currentRadius;
        public Vector2 Center => m_center;
    }
}