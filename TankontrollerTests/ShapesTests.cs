using Microsoft.Xna.Framework;
using Tankontroller.World.Shapes;
using Tankontroller.World.WorldObject;

namespace TankontrollerTests
{
    public class ShapesTests
    {
        #region PointToPoint

        [Fact]
        public void PointToPoint_SamePosition_ReportsCollisionAndVectors()
        {
            Vector2 position = new(1.0f, 2.0f);
            Transform transform1 = new(position);
            Transform transform2 = new(position);

            PointShape point1 = new(transform1);
            PointShape point2 = new(transform2);

            CollisionEvent collisionEvent = point1.Intersects(point2);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(position, collisionEvent.CollisionPosition.Value);

            collisionEvent = point2.Intersects(point1);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(position, collisionEvent.CollisionPosition.Value);
        }

        [Fact]
        public void PointToPoint_DifferentPosition_ReportsNoCollision()
        {
            Transform transform1 = new(new Vector2(0f, 0f));
            Transform transform2 = new(new Vector2(1f, 1f));

            PointShape point1 = new(transform1);
            PointShape point2 = new(transform2);

            CollisionEvent collisionEvent = point1.Intersects(point2);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = point2.Intersects(point1);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region CircleToCircle

        [Fact]
        public void CircleToCircle_Overlapping_ReportsCollisionAndVectors()
        {
            Transform circle1Transform = new(new Vector2(0f, 0f));
            Transform circle2Transform = new(new Vector2(3f, 0f));

            CircleShape circle1 = new(circle1Transform, 2.0f);
            CircleShape circle2 = new(circle2Transform, 2.0f);

            // Two circles with centers 3 units apart, radii 2 and 2 -> overlap
            CollisionEvent collisionEvent = circle1.Intersects(circle2);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(new Vector2(-1f, 0f), collisionEvent.CollisionNormal);

            collisionEvent = circle2.Intersects(circle1);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(new Vector2(1f, 0f), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void CircleToCircle_NotOverlapping_ReportsNoCollision()
        {
            Transform circle1Transform = new(new Vector2(0f, 0f));
            Transform circle2Transform = new(new Vector2(5f, 0f));

            CircleShape circle1 = new(circle1Transform, 2f);
            CircleShape circle2 = new(circle2Transform, 2f);

            CollisionEvent collisionEvent = circle1.Intersects(circle2);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = circle2.Intersects(circle1);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region AARectangletoAARectangle

        [Fact]
        public void AARectangleToAARectangle_Overlapping_ReportsCollisionAndVectors()
        {
            Transform rectangle1Transform = new(new Vector2(0f, 0f));
            Transform rectangle2Transform = new(new Vector2(3f, 0f));

            RectangleAxisAlignedShape rectangle1 = new(rectangle1Transform, new Vector2(4f, 4f)); // spans [-2,2]
            RectangleAxisAlignedShape rectangle2 = new(rectangle2Transform, new Vector2(4f, 4f)); // spans [1,5] -> overlap on x [1,2]

            CollisionEvent collisionEvent = rectangle1.Intersects(rectangle2);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            // Overlap x-range [1,2] -> midpoint x = 1.5, y-range [-2,2] -> midpoint y = 0
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangle1.WorldPosition - rectangle2.WorldPosition), collisionEvent.CollisionNormal);

            collisionEvent = rectangle2.Intersects(rectangle1);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangle2.WorldPosition - rectangle1.WorldPosition), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void AARectangleToAARectangle_NotOverlapping_ReportsNoCollision()
        {
            Transform rectangle1Transform = new(new Vector2(0f, 0f));
            Transform rectangle2Transform = new(new Vector2(3f, 0f));

            RectangleAxisAlignedShape rectangle1 = new(rectangle1Transform, new Vector2(2f, 2f)); // spans [-1,1]
            RectangleAxisAlignedShape rectangle2 = new(rectangle2Transform, new Vector2(2f, 2f)); // spans [2,4] -> not overlapping

            CollisionEvent collisionEvent = rectangle1.Intersects(rectangle2);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = rectangle2.Intersects(rectangle1);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region ORectangletoORectangle

        [Fact]
        public void ORectangleToORectangle_Overlapping_ReportsCollisionAndVectors()
        {
            Transform rectangle1Transform = new(new Vector2(0f, 0f));
            Transform rectangle2Transform = new(new Vector2(3f, 0f));

            // Both rectangles oriented with zero rotation - behaves like AABB for these tests
            RectangleOrientedShape rectangle1 = new(rectangle1Transform, new Vector2(4f, 4f));
            RectangleOrientedShape rectangle2 = new(rectangle2Transform, new Vector2(4f, 4f));

            CollisionEvent collisionEvent = rectangle1.Intersects(rectangle2);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            // Overlap x-range [1,2] -> midpoint x = 1.5, y-range [-2,2] -> midpoint y = 0
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangle1.WorldPosition - rectangle2.WorldPosition), collisionEvent.CollisionNormal);

            collisionEvent = rectangle2.Intersects(rectangle1);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(new Vector2(1.5f, 0f), collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangle2.WorldPosition - rectangle1.WorldPosition), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void ORectangleToORectangle_NotOverlapping_ReportsNoCollision()
        {
            Transform rectangle1Transform = new(new Vector2(0f, 0f));
            Transform rectangle2Transform = new(new Vector2(3f, 0f));

            RectangleOrientedShape rectangle1 = new(rectangle1Transform, new Vector2(2f, 2f)); // spans [-1,1]
            RectangleOrientedShape rectangle2 = new(rectangle2Transform, new Vector2(2f, 2f)); // spans [2,4] -> not overlapping

            CollisionEvent collisionEvent = rectangle1.Intersects(rectangle2);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = rectangle2.Intersects(rectangle1);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        [Fact]
        public void ORectangleToORectangle_RotatedOverlapping_ReportsCollisionAndVectors()
        {
            Transform rectangle1Transform = new(new Vector2(0f, 0f), 0f, Vector2.One);
            Transform rectangle2Transform = new(new Vector2(2f, 0f), 0f, Vector2.One);

            rectangle1Transform.Rotation = MathHelper.ToRadians(14.0f);
            rectangle2Transform.Rotation = MathHelper.ToRadians(-14.0f);

            RectangleOrientedShape rectangle1 = new(rectangle1Transform, new Vector2(4f, 4f));
            RectangleOrientedShape rectangle2 = new(rectangle2Transform, new Vector2(4f, 4f));

            CollisionEvent collisionEvent = rectangle1.Intersects(rectangle2);

            Vector2 expectedMidpoint = new(1.286f, 0.179f);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.True(Vector2.Distance(expectedMidpoint, collisionEvent.CollisionPosition.Value) <= float.Epsilon);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangle1.WorldPosition - rectangle2.WorldPosition), collisionEvent.CollisionNormal);

            collisionEvent = rectangle2.Intersects(rectangle1);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.True(Vector2.Distance(expectedMidpoint, collisionEvent.CollisionPosition.Value) <= float.Epsilon);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangle2.WorldPosition - rectangle1.WorldPosition), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void ORectangleToORectangle_RotatedNotOverlapping_ReportsNoCollision()
        {
            Transform rectangle1Transform = new(new Vector2(0f, 0f), 0f, Vector2.One);
            Transform rectangle2Transform = new(new Vector2(5f, 0f), 0f, Vector2.One);

            rectangle1Transform.Rotation = MathHelper.ToRadians(45.0f);
            rectangle2Transform.Rotation = MathHelper.ToRadians(-45.0f);

            RectangleOrientedShape rectangle1 = new(rectangle1Transform, new Vector2(2f, 2f));
            RectangleOrientedShape rectangle2 = new(rectangle2Transform, new Vector2(2f, 2f));

            CollisionEvent collisionEvent = rectangle1.Intersects(rectangle2);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = rectangle2.Intersects(rectangle1);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region PointToCircle

        [Fact]
        public void PointInsideCircle_ReportsCollisionAndVectors()
        {
            Transform circleTransform = new(new Vector2(0f, 0f));
            Transform pointTransform = new(new Vector2(3f, 4f)); // distance = 5

            CircleShape circle = new(circleTransform, 5f); // radius 5
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(circle);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(new Vector2(0.6f, 0.8f), collisionEvent.CollisionNormal); 

            collisionEvent = circle.Intersects(point);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(new Vector2(-0.6f, -0.8f), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void PointOutsideCircle_ReportsNoCollision()
        {
            Transform circleTransform = new(new Vector2(0f, 0f));
            Transform pointTransform = new(new Vector2(6f, 0f)); // outside radius 5

            CircleShape circle = new(circleTransform, 5f);
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(circle);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = circle.Intersects(point);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region PointToAARectangle

        [Fact]
        public void PointInsideAARectangle_ReportsCollisionAndVectors()
        {
            Transform rectangleTransform = new(new Vector2(0f, 0f));
            Vector2 rectangleSize = new(4f, 4f);
            Transform pointTransform = new(new Vector2(1f, 1f));  // inside a 4x4 centered at (0,0)

            RectangleAxisAlignedShape rectangle = new(rectangleTransform, rectangleSize);
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(rectangle);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(pointTransform.Position - rectangleTransform.Position), collisionEvent.CollisionNormal);

            collisionEvent = rectangle.Intersects(point);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangleTransform.Position - pointTransform.Position), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void PointOutsideAARectangle_ReportsNoCollision()
        {
            Transform rectangleTransform = new(new Vector2(0f, 0f));
            Vector2 rectangleSize = new(4f, 4f);
            Transform pointTransform = new(new Vector2(3f, 3f)); // outside a 4x4 centered at (0,0)

            RectangleAxisAlignedShape rectangle = new(rectangleTransform, rectangleSize);
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(rectangle);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = rectangle.Intersects(point);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region PointToORectangle

        [Fact]
        public void PointToORectangle_InsideRotatedRectangle_ReportsCollisionAndVectors()
        {
            Transform rectangleTransform = new(new Vector2(0f, 0f), 0f, Vector2.One);
            rectangleTransform.Rotation = MathHelper.ToRadians(30.0f);

            Transform pointTransform = new(new Vector2(1f, 0.5f));

            RectangleOrientedShape rectangle = new(rectangleTransform, new Vector2(4f, 4f));
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(rectangle);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(pointTransform.Position - rectangleTransform.Position), collisionEvent.CollisionNormal);

            collisionEvent = rectangle.Intersects(point);

            Assert.True(collisionEvent.HasCollided);
            Assert.True(collisionEvent.CollisionPosition.HasValue);
            Assert.Equal(pointTransform.Position, collisionEvent.CollisionPosition.Value);
            Assert.True(collisionEvent.CollisionNormal.HasValue);
            Assert.Equal(Vector2.Normalize(rectangleTransform.Position - pointTransform.Position), collisionEvent.CollisionNormal);
        }

        [Fact]
        public void PointToORectangle_OutsideRotatedRectangle_ReportsNoCollision()
        {
            Transform rectangleTransform = new(new Vector2(0f, 0f), 0f, Vector2.One);
            rectangleTransform.Rotation = MathHelper.ToRadians(30.0f);

            Transform pointTransform = new(new Vector2(3f, 3f));

            RectangleOrientedShape rectangle = new(rectangleTransform, new Vector2(2f, 2f));
            PointShape point = new(pointTransform);

            CollisionEvent collisionEvent = point.Intersects(rectangle);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);

            collisionEvent = rectangle.Intersects(point);

            Assert.False(collisionEvent.HasCollided);
            Assert.False(collisionEvent.CollisionPosition.HasValue);
            Assert.False(collisionEvent.CollisionNormal.HasValue);
        }

        #endregion

        #region CircleToAARectangle

        #endregion

        #region CircleToOBB

        #endregion

        #region AARectangleToORectangle

        #endregion
    }
}
