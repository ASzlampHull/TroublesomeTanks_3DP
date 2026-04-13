using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Tankontroller.World;
using Tankontroller.World.Gameplay;
using Xunit; // Use xUnit namespace

namespace TankontrollerTests
{
    public class DeathRingTests
    {
        [Theory]
        [InlineData(100f, false)] // Well above activation time (Inactive)
        [InlineData(75f, false)]  // Still above activation time (Inactive)
        [InlineData(45f, true)]   // Exactly at activation time (Active)
        [InlineData(20f, true)]   // Below activation time (Active)
        [InlineData(0f, true)]    // Match time expired (Active)
        public void DeathRing_Activation_StateReflectsRemainingTime(float remainingMatchSeconds, bool expectedIsActive)
        {
            // Arrange
            Rectangle playArea = new Rectangle(0, 0, 1000, 1000);
            DeathRing deathRing = new DeathRing(playArea);
            List<Tank> dummyTanks = new List<Tank>();

            // Act
            deathRing.Update(deltaSeconds: 1f, remainingMatchSeconds: remainingMatchSeconds, tanks: dummyTanks);
            
            // Assert
            // Explicitly use Xunit.Assert to avoid ambiguity
            Xunit.Assert.Equal(expectedIsActive, deathRing.IsActive());
        }
    }
}