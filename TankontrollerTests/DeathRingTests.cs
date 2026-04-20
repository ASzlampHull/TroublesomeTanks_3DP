using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Tankontroller.World;
using Tankontroller.World.Gameplay;
using Xunit;

namespace TankontrollerTests
{
    // Simple test double bypassing MonoGame entirely.
    public class MockTank : IDeathRingTarget
    {
        public Vector2 Position { get; set; }
        public int Health { get; private set; }

        public MockTank(Vector2 position, int initialHealth = 100)
        {
            Position = position;
            Health = initialHealth;
        }

        public Vector2 GetWorldPosition() => Position;
        
        public void TakeDamage()
        {
            Health--;
        }
        
        public void OffsetPosition(Vector2 offset)
        {
            Position += offset;
        }
    }

    public class DeathRingTests
    {
        // Default values assumed without a DGS config file loaded:
        // Play area: 1000x1000
        // Activation Time: 45f
        // Duration: 30f
        // Start Radius: 1000 * 1.2f = 1200f
        // End Radius: 1000 * 0.2f = 200f
        // Grace Seconds: 1f
        // DPS: 10f
        private const float ACTIVATION_TIME = 45f;
        private const float DURATION = 30f;
        private const float START_RADIUS = 1200f;
        private const float END_RADIUS = 200f;
        private const float GRACE_SECONDS = 1f;
        private const float DAMAGE_PER_SECOND = 10f;

        [Fact]
        public void TestRadiusAtActivationStart_ShouldBeStartRadius()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);
            var dummyTanks = new List<IDeathRingTarget>();

            // Act: Trigger activation at exactly the activation time (0 seconds elapsed into the shrink)
            // CurrentMatchTime counts down. When matchTime == ACTIVATION_TIME, shrinking just begins.
            deathRing.Update(0f, ACTIVATION_TIME, dummyTanks);

            // Assert: The radius should exactly match the START_RADIUS
            Xunit.Assert.True(deathRing.IsActive());
            Xunit.Assert.Equal(START_RADIUS, deathRing.CurrentRadius);
        }

        [Fact]
        public void TestRadiusAtQuarterDuration_ShouldBeQuarterShrunk()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);
            var dummyTanks = new List<IDeathRingTarget>();

            // Act 1: Trigger activation
            deathRing.Update(0f, ACTIVATION_TIME, dummyTanks);

            // Act 2: Advance time by exactly 25% of the duration
            float quarterDuration = DURATION * 0.25f; // 7.5 seconds
            
            // Note: Match timer counts down. Time remaining = ACTIVATION_TIME - timeElapsed
            deathRing.Update(quarterDuration, ACTIVATION_TIME - quarterDuration, dummyTanks);

            // Assert: The radius should be shrunk by exactly 25% of the total shrink amount
            float shrinkAmount = (START_RADIUS - END_RADIUS) * 0.25f; // (1200 - 200) * 0.25 = 250
            float expectedRadius = START_RADIUS - shrinkAmount;       // 1200 - 250 = 950
            
            Xunit.Assert.Equal(expectedRadius, deathRing.CurrentRadius);
        }

        [Fact]
        public void TestRadiusAtHalfDuration_ShouldBeExactlyMidpoint()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);
            var dummyTanks = new List<IDeathRingTarget>();

            // Act 1: Trigger activation
            deathRing.Update(0f, ACTIVATION_TIME, dummyTanks);

            // Act 2: Advance time by exactly 50% of the duration
            float halfDuration = DURATION * 0.5f; // 15 seconds
            deathRing.Update(halfDuration, ACTIVATION_TIME - halfDuration, dummyTanks);

            // Assert: The radius should be exactly halfway between Start and End radii
            float expectedRadius = MathHelper.Lerp(START_RADIUS, END_RADIUS, 0.5f); // 700
            
            Xunit.Assert.Equal(expectedRadius, deathRing.CurrentRadius);
        }

        [Fact]
        public void TestRadiusAtExactEnd_ShouldBeEndRadius()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);
            var dummyTanks = new List<IDeathRingTarget>();

            // Act 1: Trigger activation
            deathRing.Update(0f, ACTIVATION_TIME, dummyTanks);

            // Act 2: Advance time by exactly the full duration
            deathRing.Update(DURATION, ACTIVATION_TIME - DURATION, dummyTanks);

            // Assert: The radius should exactly match the END_RADIUS
            Xunit.Assert.Equal(END_RADIUS, deathRing.CurrentRadius);
        }

        [Fact]
        public void TestRadiusPastDuration_ShouldClampToEndRadius()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);
            var dummyTanks = new List<IDeathRingTarget>();

            // Act 1: Trigger activation
            deathRing.Update(0f, ACTIVATION_TIME, dummyTanks);

            // Act 2: Advance time massively past the duration limit (e.g., 50 seconds past duration)
            float wayPastDuration = DURATION + 50f;
            deathRing.Update(wayPastDuration, ACTIVATION_TIME - wayPastDuration, dummyTanks);

            // Assert: The math must clamp and never shrink the safe zone below the END_RADIUS
            Xunit.Assert.Equal(END_RADIUS, deathRing.CurrentRadius);
        }

        [Fact]
        public void TestGracePeriod_TankInsideGracePeriod_ShouldTakeNoDamage()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000); // Center is 500, 500
            var deathRing = new DeathRing(playArea);

            var mockTank = new MockTank(new Vector2(5000f, 5000f), 100);
            var tanks = new List<IDeathRingTarget> { mockTank };

            // Act: Activate the ring
            deathRing.Update(0f, ACTIVATION_TIME, tanks);

            // Advance by just under the required grace period
            float timeOutside = GRACE_SECONDS * 0.9f;
            deathRing.Update(timeOutside, ACTIVATION_TIME - timeOutside, tanks);

            // Assert: The tank's health should be untouched
            Xunit.Assert.Equal(100, mockTank.Health);
        }

        [Fact]
        public void TestGracePeriod_TankBeyondGracePeriod_ShouldTakeDamage()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);

            var mockTank = new MockTank(new Vector2(5000f, 5000f), 100);
            var tanks = new List<IDeathRingTarget> { mockTank };

            // Act 1: Activate the ring
            deathRing.Update(0f, ACTIVATION_TIME, tanks);

            // Act 2: Simulate near the grace limit
            deathRing.Update(0.99f, ACTIVATION_TIME - 0.99f, tanks);
            Xunit.Assert.Equal(100, mockTank.Health); // Double check still healthy

            // Act 3: Apply the frame that breaches the grace limit
            float damageDelta = 0.5f;
            deathRing.Update(damageDelta, ACTIVATION_TIME - (0.99f + damageDelta), tanks);

            // Expected damage reflects the delta timeframe applied once past the GRACE_SECONDS limit
            int expectedDamageTaken = (int)(DAMAGE_PER_SECOND * damageDelta);

            // Assert: Tank should have properly received exactly the expected damage
            Xunit.Assert.Equal(100 - expectedDamageTaken, mockTank.Health);
        }

        [Fact]
        public void TestGracePeriod_TankReenteringSafeZone_ShouldResetGracePeriod()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);

            // Tank positioned firmly out of bounds at 5000, 5000 (Safe zone center is 500,500)
            var mockTank = new MockTank(new Vector2(5000f, 5000f), 100);
            var tanks = new List<IDeathRingTarget> { mockTank };

            // Act 1: Activate Ring
            deathRing.Update(0f, ACTIVATION_TIME, tanks);

            // Act 2: Keep tank outside very close to the grace limit (0.9s out of 1.0s limit)
            deathRing.Update(0.9f, ACTIVATION_TIME - 0.9f, tanks);
            Xunit.Assert.Equal(100, mockTank.Health);

            // Act 3: Move tank back into the center (safe zone space)
            mockTank.OffsetPosition(new Vector2(-4500f, -4500f));
            
            // Allow an arbitrary frame to pass where damage WOULD be triggered if it hadn't correctly reset
            deathRing.Update(0.2f, ACTIVATION_TIME - 1.1f, tanks);
            Xunit.Assert.Equal(100, mockTank.Health);

            // Act 4: Snap the tank outside back into the exact same dangerous location
            mockTank.OffsetPosition(new Vector2(4500f, 4500f));
            
            // Advance by another slightly-under-limit 0.9s (Totalling 1.8s outside unadjusted). 
            deathRing.Update(0.9f, ACTIVATION_TIME - 2.0f, tanks);

            // Assert: With the logic successfully resetting upon entry, no damage should compile
            Xunit.Assert.Equal(100, mockTank.Health);
        }
    }
}