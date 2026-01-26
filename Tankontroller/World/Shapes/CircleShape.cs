using Microsoft.Xna.Framework;
using System;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class CircleShape : CollisionShape
    {
        public float Radius { get; private set; } = 1.0f;

        public CircleShape(Transform pOwner, float pRadius, bool pEnabled = true) : base(pOwner, pEnabled)
        {
            Radius = pRadius;
        }

        public CircleShape(Transform pOwner, float pRadius, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
            Radius = pRadius;
        }

        public override CollisionEvent Intersects(CollisionShape other)
        {
            return other switch
            {
                PointShape point => Intersects(point),
                CircleShape circleShape => Intersects(circleShape),
                _ => throw new NotImplementedException($"Intersection with shape {this} and {other} is not implemented."),
            };
        }

        /// <summary>
        /// Check for intersection with a point shape - if the point is inside the circle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself)
        /// 2. The normal of the collision (pointing away from the point, into the circle) </returns>
        public CollisionEvent Intersects(PointShape point)
        {
            CollisionEvent collisionEvent = point.Intersects(this);
            collisionEvent.CollisionNormal = -collisionEvent.CollisionNormal; // Invert normal to point into the circle
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with another circle shape - if the circles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the two circles)
        /// 2. The normal of the collision (pointing away from the other circle) </returns>
        public CollisionEvent Intersects(CircleShape other)
        {
            Vector2 difference = WorldPosition - other.WorldPosition;
            float radiusSum = Radius + other.Radius;
            float distanceSquared = difference.LengthSquared();

            if (distanceSquared <= radiusSum * radiusSum)
            {
                float distance = (float)Math.Sqrt(distanceSquared);

                // Calculate the collision normal, avoiding division by zero
                Vector2 collisionNormal = distance > 0f ? Vector2.Normalize(difference) : new Vector2(1f, 0);

                // Closest point on the other circle's perimeter to this circle
                Vector2 pointOnOther = other.WorldPosition + collisionNormal * other.Radius;

                // Closest point on this circle's perimeter to the other circle
                Vector2 pointOnThis = WorldPosition - collisionNormal * Radius;

                // Collision position is the midpoint between the two closest points (the overlap area center)
                Vector2 collisionPosition = (pointOnOther + pointOnThis) / 2f;

                return new CollisionEvent(true, collisionPosition, collisionNormal);
            }
            return new CollisionEvent(false);
        }
    }
}
