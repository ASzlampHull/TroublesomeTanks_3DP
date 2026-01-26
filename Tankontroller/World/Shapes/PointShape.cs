using Microsoft.Xna.Framework;
using System;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class PointShape : CollisionShape
    {
        public PointShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled) { }
        public PointShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled) { }

        public override CollisionEvent Intersects(CollisionShape pOther)
        {
            return pOther switch
            {
                PointShape point => IntersectsPoint(point),
                CircleShape circle => IntersectsCircle(circle),
                RectangleAxisAlignedShape rectangleAligned => IntersectsAlignedRectangle(rectangleAligned),
                RectangleOrientedShape rectangleOriented => IntersectsOrientedRectangle(rectangleOriented),
                _ => throw new NotImplementedException($"Intersection with shape {this} and {pOther} is not implemented."),
            };
        }

        /// <summary>
        /// Check for intersection with another point shape - if they occupy the same position.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            if (Vector2.DistanceSquared(WorldPosition, pPoint.WorldPosition) <= float.Epsilon)
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
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            Vector2 difference = WorldPosition - pCircle.WorldPosition;
            if (difference.LengthSquared() <= pCircle.Radius * pCircle.Radius)
            {
                return new CollisionEvent(true, WorldPosition, Vector2.Normalize(difference));
            }
            return new CollisionEvent(false);
        }

        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            return new CollisionEvent(false);
        }

        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            return new CollisionEvent(false);
        }
    }
}
