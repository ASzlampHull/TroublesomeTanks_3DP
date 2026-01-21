using Microsoft.Xna.Framework;
using System;
using Tankontroller.Managers;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shape
{
    internal class PointShape : Shape
    {
        public PointShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled) { }
        public PointShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled) { }

        public override CollisionEvent Intersects(Shape other)
        {
            switch (other)
            {
                case PointShape point:
                    return Intersects(point);
                //case CircleShape circleShape:
                //    Vector2 difference = this.WorldPosition - circleShape.WorldPosition;
                //    return difference.LengthSquared() <= circleShape.Radius * circleShape.Radius;
                //case RectangleShape rectangleShape:
                //    Vector2 worldPos = this.WorldPosition;
                //    Vector2 rectPos = rectangleShape.WorldPosition;
                //    Vector2 rectSize = rectangleShape.Size;
                //    return (worldPos.X >= rectPos.X && worldPos.X <= rectPos.X + rectSize.X) &&
                //           (worldPos.Y >= rectPos.Y && worldPos.Y <= rectPos.Y + rectSize.Y);
                default:
                    throw new NotImplementedException("Intersection with this shape type is not implemented.");
            }
        }

        public CollisionEvent Intersects(PointShape other)
        {
            if (Vector2.DistanceSquared(WorldPosition, other.WorldPosition) <= float.Epsilon)
            {
                return new CollisionEvent(true, WorldPosition);
            }
            return new CollisionEvent(false);
        }

        public CollisionEvent Intersects(CircleShape circle)
        {
            //Vector2 difference = this.WorldPosition - circle.WorldPosition;
            //if (difference.LengthSquared() <= circle.Radius * circle.Radius)
            //{
            //    return new CollisionEvent(true, this.WorldPosition);
            //}
            return new CollisionEvent(false);
        }
    }
}
