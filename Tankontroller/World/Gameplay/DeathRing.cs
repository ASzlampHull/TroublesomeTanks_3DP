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
        private readonly float ACTIVATION_TIME;    // seconds before match end to activate
        private readonly float DURATION;          // how long shrink lasts
        private readonly float START_RADIUS;       // computed from play area if not set in DGS
        private readonly float END_RADIUS;         // final safe radius
        private readonly float DAMAGE_PER_SECOND;   // DPS applied outside safe zone
        private readonly float GRACE_SECONDS;      // Seconds a tank can survive outside the ring before the first "tick" of damage.
        private readonly float START_THICKNESS;    // NEW: Initial ring thickness
        private readonly float END_THICKNESS;      // NEW: Final ring thickness

        // State
        private readonly Vector2 CENTER;
        private float mElapsedSinceStart = 0f;
        private float mCurrentRadius;              // Inner edge (safe zone boundary)
        private float mCurrentThickness;           // NEW: Current ring thickness
        private bool mActive = false;
        private const float DEATH_ZONE_MASK_SIZE = 30f;     // Adjust this to increase the size of the ring mask and it's surrounding rectangle

        // Per-tank state
        private readonly Dictionary<Tank, float> OUTSIDE_TIME = new();
        private readonly Dictionary<Tank, float> DAMAGE_ACCUMULATOR = new();

        // DEBUG: Timer for periodic logging
        private float mDebugLogTimer = 0f;
        private const float DEBUG_LOG_INTERVAL = 1f; // Log every 1 second

        public DeathRing(Rectangle playArea)
        {
            /// <summary>
            /// sets a float from DGS or returns default if not found or invalid
            /// </summary>
            float SafeFloat(string key, float def)
            {
                try { return DGS.Instance.GetFloat(key); } catch { return def; }
            }

            ACTIVATION_TIME = SafeFloat("DEATH_RING_ACTIVATION_TIME", 45f);
            DURATION = SafeFloat("DEATH_RING_DURATION", Math.Max(1f, ACTIVATION_TIME));
            DAMAGE_PER_SECOND = SafeFloat("DEATH_RING_DPS", 10f);
            GRACE_SECONDS = SafeFloat("DEATH_RING_GRACE", 1f);
            
            // NEW: Load thickness configuration
            START_THICKNESS = SafeFloat("DEATH_RING_START_THICKNESS", 50f);
            END_THICKNESS = SafeFloat("DEATH_RING_END_THICKNESS", 300f);

            CENTER = new Vector2(playArea.X + playArea.Width / 2f, playArea.Y + playArea.Height / 2f);

            float defaultStart = MathF.Max(playArea.Width, playArea.Height) * 1.2f;
            float configuredStart = SafeFloat("DEATH_RING_START_RADIUS", defaultStart);
            START_RADIUS = configuredStart > 0 ? configuredStart : defaultStart;

            float defaultEnd = MathF.Max(playArea.Width, playArea.Height) * 0.3f;
            float configuredEnd = SafeFloat("DEATH_RING_END_RADIUS", defaultEnd);
            END_RADIUS = configuredEnd > 0 ? configuredEnd : defaultEnd;  

            mCurrentRadius = START_RADIUS;
            mCurrentThickness = START_THICKNESS;  
        }

        /// <summary>
        /// Update ring state. Call every frame.
        /// - deltaSeconds: frame delta
        /// - remainingMatchSeconds: seconds left in match (countdown)
        /// - tanks: list of tanks (pass all active tanks)
        /// </summary>
        public void Update(float deltaSeconds, float remainingMatchSeconds, List<Tank> tanks)
        {
            if (!mActive)
            {
                if (remainingMatchSeconds <= ACTIVATION_TIME)
                {
                    mActive = true;
                    // Align elapsed so the ring progress corresponds to time since activation
                    mElapsedSinceStart = MathF.Max(0f, ACTIVATION_TIME - remainingMatchSeconds);
                }
                else
                {
                    return;
                }
            }

            // Progress shrink (linear), clamped to not shrink below endRadius
            mElapsedSinceStart += deltaSeconds;
            float t = (DURATION <= 0f) ? 1f : MathF.Min(1f, mElapsedSinceStart / DURATION);

            // NEW: Update thickness first , visually doesn't work rn haha
            mCurrentThickness = MathHelper.Lerp(START_THICKNESS, END_THICKNESS, t);

            // Calculate base radius, then subtract thickness to get inner edge (safe zone boundary)
            float baseRadius = MathHelper.Lerp(START_RADIUS, END_RADIUS, t);
            mCurrentRadius = MathHelper.Clamp(baseRadius - mCurrentThickness, END_RADIUS - END_THICKNESS, START_RADIUS);

            // DEBUG: Periodic logging
            mDebugLogTimer += deltaSeconds;
            if (mDebugLogTimer >= DEBUG_LOG_INTERVAL)
            {
                mDebugLogTimer -= DEBUG_LOG_INTERVAL;
                LogDebugInfo(tanks);
            }

            // Damage application: continuous DPS after grace
            foreach (var tank in tanks)
            {
                if (tank == null) continue;
                Vector2 tankPos = tank.GetWorldPosition();
                float dist = Vector2.Distance(tankPos, CENTER);
                bool outside = dist > mCurrentRadius;

                if (outside)
                {
                    if (!OUTSIDE_TIME.ContainsKey(tank)) OUTSIDE_TIME[tank] = 0f;
                    OUTSIDE_TIME[tank] += deltaSeconds;

                    if (OUTSIDE_TIME[tank] >= GRACE_SECONDS)
                    {
                        if (!DAMAGE_ACCUMULATOR.ContainsKey(tank)) DAMAGE_ACCUMULATOR[tank] = 0f;
                        DAMAGE_ACCUMULATOR[tank] += DAMAGE_PER_SECOND * deltaSeconds;

                        float acc = DAMAGE_ACCUMULATOR[tank];
                        int wholeHits = (int)MathF.Floor(acc);
                        if (wholeHits > 0)
                        {
                            for (int i = 0; i < wholeHits; i++)
                            {
                                tank.TakeDamage();
                            }
                            DAMAGE_ACCUMULATOR[tank] = acc - wholeHits;
                        }
                    }
                }
                else
                {
                    // Reset when tank returns inside
                    if (OUTSIDE_TIME.ContainsKey(tank)) OUTSIDE_TIME.Remove(tank);
                    if (DAMAGE_ACCUMULATOR.ContainsKey(tank)) DAMAGE_ACCUMULATOR.Remove(tank);
                }
            }

            // Prune state for tanks that no longer exist in supplied list (avoid leaks)
            var toRemove = OUTSIDE_TIME.Keys.Where(k => !tanks.Contains(k)).ToList();
            foreach (var k in toRemove)
            {
                OUTSIDE_TIME.Remove(k);
                DAMAGE_ACCUMULATOR.Remove(k);
            }
        }

        /// <summary>
        /// DEBUG: Log death ring and tank information every second
        /// </summary>
        private void LogDebugInfo(List<Tank> tanks)
        {
            // Calculate visual (outer) radius
            float visualRadius = mCurrentRadius + mCurrentThickness;

            // Log death ring state
            System.Diagnostics.Debug.WriteLine("=== DEATH RING DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Ring State -> CurrentRadius (Inner): {mCurrentRadius:F2}, Thickness: {mCurrentThickness:F2}, VisualRadius (Outer): {visualRadius:F2}");
            System.Diagnostics.Debug.WriteLine($"Ring Center: ({CENTER.X:F1}, {CENTER.Y:F1})");
            System.Diagnostics.Debug.WriteLine("");

            // Log per-tank information
            System.Diagnostics.Debug.WriteLine("Tank Data:");
            int tankIndex = 1;
            foreach (var tank in tanks)
            {
                if (tank == null) continue;

                Vector2 tankPos = tank.GetWorldPosition();
                float distFromCenter = Vector2.Distance(tankPos, CENTER);
                bool isOutside = distFromCenter > mCurrentRadius;
                float distanceFromEdge = distFromCenter - mCurrentRadius;

                System.Diagnostics.Debug.WriteLine($"  Tank {tankIndex}:");
                System.Diagnostics.Debug.WriteLine($"    Position: ({tankPos.X:F1}, {tankPos.Y:F1})");
                System.Diagnostics.Debug.WriteLine($"    Distance from Center: {distFromCenter:F2}");
                System.Diagnostics.Debug.WriteLine($"    Distance from Safe Edge: {distanceFromEdge:F2} ({(isOutside ? "OUTSIDE" : "INSIDE")})");
                
                if (OUTSIDE_TIME.ContainsKey(tank))
                {
                    System.Diagnostics.Debug.WriteLine($"    Time Outside: {OUTSIDE_TIME[tank]:F2}s");
                }
                if (DAMAGE_ACCUMULATOR.ContainsKey(tank))
                {
                    System.Diagnostics.Debug.WriteLine($"    Damage Accumulator: {DAMAGE_ACCUMULATOR[tank]:F2}");
                }

                tankIndex++;
            }
            System.Diagnostics.Debug.WriteLine("========================");
            System.Diagnostics.Debug.WriteLine("");
        }

        /// <summary>
        /// Draw ring visuals. Call between SpriteBatch.Begin/End.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!mActive) return;

            // color for ring (RGBA). adjust alpha to taste.
            Color ringColor = new Color(200, 30, 30, 200);

            DrawUtilities.DrawDeathZone(
                spriteBatch,
                CENTER,
                mCurrentRadius,
                ringColor,
                DEATH_ZONE_MASK_SIZE
            );

            //DEBUG Circle to show inner edge (safe zone boundary)
            DrawUtilities.DrawCircle(spriteBatch, CENTER, mCurrentRadius, Color.Blue * 0.5f);
        }

        public bool IsActive() => mActive;
        public float CurrentRadius => mCurrentRadius;
        public Vector2 Center => CENTER;
    }
}