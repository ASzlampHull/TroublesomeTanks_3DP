using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Tankontroller.World;
using Tankontroller.World.Gameplay;
using Xunit;

namespace TankontrollerTests
{
    public class DeathRingTests
    {
        // Default values assumed without a DGS config file loaded:
        // Play area: 1000x1000
        // Activation Time: 45f
        // Duration: 30f
        // Start Radius: 1000 * 1.2f = 1200f
        // End Radius: 1000 * 0.2f = 200f
        private const float ACTIVATION_TIME = 45f;
        private const float DURATION = 30f;
        private const float START_RADIUS = 1200f;
        private const float END_RADIUS = 200f;

        [Fact]
        public void TestRadiusAtActivationStart_ShouldBeStartRadius()
        {
            // Arrange
            var playArea = new Rectangle(0, 0, 1000, 1000);
            var deathRing = new DeathRing(playArea);
            var dummyTanks = new List<Tank>();

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
            var dummyTanks = new List<Tank>();

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
            var dummyTanks = new List<Tank>();

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
            var dummyTanks = new List<Tank>();

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
            var dummyTanks = new List<Tank>();

            // Act 1: Trigger activation
            deathRing.Update(0f, ACTIVATION_TIME, dummyTanks);

            // Act 2: Advance time massively past the duration limit (e.g., 50 seconds past duration)
            float wayPastDuration = DURATION + 50f;
            deathRing.Update(wayPastDuration, ACTIVATION_TIME - wayPastDuration, dummyTanks);

            // Assert: The math must clamp and never shrink the safe zone below the END_RADIUS
            Xunit.Assert.Equal(END_RADIUS, deathRing.CurrentRadius);
        }
    }
}