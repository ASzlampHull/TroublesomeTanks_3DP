

using Microsoft.Xna.Framework;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace TankontrollerTests
{
    public class ShapesTests
    {
        #region PointShape

        [Fact]
        public void PointToPoint_SamePosition_ReportsCollisionAndVectors()
        {
            Vector2 position = new(1.0f, 2.0f);
            Transform transform1 = new(position);
            Transform transform2 = new(position);

            PointShape point1 = new(transform1);
            PointShape point2 = new(transform2);

            CollisionEvent collisionEvent = point1.Intersects(point2);

            Assert.True(collisionEvent.HasCollided); // Collision should be detected
            Assert.True(collisionEvent.CollisionPosition.HasValue); // Collision position should be defined
            Assert.False(collisionEvent.CollisionNormal.HasValue); // No normal for point-point collision
            Assert.Equal(position, collisionEvent.CollisionPosition.Value); // Collision position should match the point position
        }

        [Fact]
        public void PointToPoint_DifferentPosition_ReportsNoCollision()
        {
            Transform transform1 = new(new Vector2(0f, 0f));
            Transform transform2 = new(new Vector2(1f, 1f));

            PointShape point1 = new(transform1);
            PointShape point2 = new(transform2);

            CollisionEvent collisionEvent = point1.Intersects(point2);

            Assert.False(collisionEvent.HasCollided); // No collision should be detected
            Assert.False(collisionEvent.CollisionPosition.HasValue); // No collision position
            Assert.False(collisionEvent.CollisionNormal.HasValue); // No normal
        }

        [Fact]
        public void PointInsideCircle_ReportsCollisionAndVectors()
        {
            Transform circleTransform = new(new Vector2(0f, 0f));
            Transform pointTransform = new(new Vector2(3f, 4f)); // distance = 5

            CircleShape circle = new(circleTransform, 5f); // radius 5
            PointShape point = new(pointTransform);

            // PointShape.Intersects(CircleShape) should detect the point on the edge as a collision
            CollisionEvent collisionEvent = point.Intersects(circle);

            Assert.True(collisionEvent.HasCollided); // Collision should be detected
            Assert.True(collisionEvent.CollisionPosition.HasValue); // Collision position should be defined
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value); // Collision position should match the point position
            Assert.True(collisionEvent.CollisionNormal.HasValue); // Normal should be defined
            Assert.Equal(new Vector2(0.6f, 0.8f), collisionEvent.CollisionNormal); // Normal should point from circle center to point
        }

        [Fact]
        public void PointOutsideCircle_ReportsNoCollision()
        {
            Transform circleTransform = new(new Vector2(0f, 0f));
            Transform pointTransform = new(new Vector2(6f, 0f)); // outside radius 5

            CircleShape circle = new(circleTransform, 5f);
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(circle);

            Assert.False(collisionEvent.HasCollided); // No collision should be detected
            Assert.False(collisionEvent.CollisionPosition.HasValue); // No collision position
            Assert.False(collisionEvent.CollisionNormal.HasValue); // No normal
        }

        #endregion PointShape

        #region CircleShape

        [Fact]
        public void CircleToPoint_SymmetricalToInvertedNormal_PointInsideCircle()
        {
            Transform circleTransform = new(new Vector2(0f, 0f));
            Transform pointTransform = new(new Vector2(2f, 0f));

            CircleShape circle = new(circleTransform, 3f);
            PointShape point = new(pointTransform);

            // This test expects CircleShape.Intersects(PointShape) to behave symmetrically to PointShape.Intersects(CircleShape)
            CollisionEvent collisionEvent = circle.Intersects(point);

            Assert.True(collisionEvent.HasCollided); // Collision should be detected
            Assert.True(collisionEvent.CollisionPosition.HasValue); // Collision position should be defined
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value); // Collision position should match the point position
            Assert.True(collisionEvent.CollisionNormal.HasValue); // Normal should be defined
            Assert.Equal(new Vector2(-1f, 0f), collisionEvent.CollisionNormal); // Normal should point from point to circle center (opposite of point-to-circle)
        }

        [Fact]
        public void CircleToCircle_NotOverlapping_ReportsNoCollision()
        {
            Transform circle1Transform = new(new Vector2(0f, 0f));
            Transform circle2Transform = new(new Vector2(5f, 0f));

            CircleShape circle1 = new(circle1Transform, 2f);
            CircleShape circle2 = new(circle2Transform, 2f);

            CollisionEvent collisionEvent = circle1.Intersects(circle2);

            Assert.False(collisionEvent.HasCollided); // No collision should be detected
            Assert.False(collisionEvent.CollisionPosition.HasValue); // No collision position
            Assert.False(collisionEvent.CollisionNormal.HasValue); // No normal
        }

        [Fact]
        public void CircleToCircle_Overlapping_ReportsCollision()
        {
            Transform circle1Transform = new(new Vector2(0f, 0f));
            Transform circle2Transform = new(new Vector2(3f, 0f));

            CircleShape circle1 = new(circle1Transform, 2.0f);
            CircleShape circle2 = new(circle2Transform, 2.0f);

            // Two circles with centers 3 units apart, radii 2 and 2 -> overlap (2+2 > 3)
            CollisionEvent collisionEvent = circle1.Intersects(circle2);

            Assert.True(collisionEvent.HasCollided); // Collision should be detected
            Assert.True(collisionEvent.CollisionPosition.HasValue); // Collision position should be defined
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value); // Collision position should be midpoint of overlap
            Assert.True(collisionEvent.CollisionNormal.HasValue); // Normal should be defined
            Assert.Equal(new Vector2(-1f, 0f), collisionEvent.CollisionNormal); // Normal should point from circle1 to circle2
        }

        #endregion CircleShape
    }
}
