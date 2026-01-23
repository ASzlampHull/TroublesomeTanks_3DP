using Microsoft.Xna.Framework;
using System;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class PointShape : Shape
    {
        public PointShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled) { }
        public PointShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled) { }

        public override CollisionEvent Intersects(Shape other)
        {
            return other switch
            {
                PointShape point => Intersects(point),
                CircleShape circleShape => Intersects(circleShape),
                _ => throw new NotImplementedException($"Intersection with shape {this} and {other} is not implemented."),
            };
        }

        /// <summary>
        /// Check for intersection with another point shape - if they occupy the same position.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself). </returns>
        public CollisionEvent Intersects(PointShape other)
        {
            if (Vector2.DistanceSquared(WorldPosition, other.WorldPosition) <= float.Epsilon)
            {
                return new CollisionEvent(true, WorldPosition);
            }
            return new CollisionEvent(false);
        }

        /// <summary>
        /// Check for intersection with a circle shape - if the point is inside the circle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself)
        /// 2. The normal of the collision (pointing away from the circle) </returns>
        public CollisionEvent Intersects(CircleShape circle)
        {
            Vector2 difference = WorldPosition - circle.WorldPosition;
            if (difference.LengthSquared() <= circle.Radius * circle.Radius)
            {
                return new CollisionEvent(true, WorldPosition, Vector2.Normalize(difference));
            }
            return new CollisionEvent(false);
        }
    }
}
