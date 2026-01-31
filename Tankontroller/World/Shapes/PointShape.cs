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
                Vector2 normal = NormalizeZeroSafe(difference);
                return new CollisionEvent(true, WorldPosition, normal);
            }
            return new CollisionEvent(false);
        }

        /// <summary>
        /// Checks for intersection with an axis-aligned rectangle shape - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing away from the rectangle center). </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            // Build AABB min/max from rectangle center and half-extents
            Vector2 rectangleMin = pRectangleAligned.Min;
            Vector2 rectangleMax = pRectangleAligned.Max;

            Vector2 pointPosition = WorldPosition;

            // Check if point is inside AABB and report collision event
            if (pointPosition.X >= rectangleMin.X && 
                pointPosition.X <= rectangleMax.X && 
                pointPosition.Y >= rectangleMin.Y && 
                pointPosition.Y <= rectangleMax.Y)
            {
                Vector2 normal = NormalizeZeroSafe(pointPosition - pRectangleAligned.WorldPosition);
                return new CollisionEvent(true, pointPosition, normal);
            }

            return new CollisionEvent(false);
        }

        /// <summary>
        /// Checks for intersection with an oriented rectangle shape - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing away from the rectangle center). </returns>
        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            // World-space point
            Vector2 pointWorld = WorldPosition;
            Vector2 pointLocal = pointWorld - pRectangleOriented.WorldPosition;

            // Rotate by negative world rotation to align rectangle axes with world axes
            float minusRotation = -pRectangleOriented.WorldRotation;
            float cos = (float)Math.Cos(minusRotation);
            float sin = (float)Math.Sin(minusRotation);
            Vector2 localSpacePoint = new(
                pointLocal.X * cos - pointLocal.Y * sin,
                pointLocal.X * sin + pointLocal.Y * cos
            );

            Vector2 halfExtents = pRectangleOriented.HalfExtents;

            // Check if local point is within the rectangle's half-extents
            if (localSpacePoint.X >= -halfExtents.X && 
                localSpacePoint.X <= halfExtents.X && 
                localSpacePoint.Y >= -halfExtents.Y && 
                localSpacePoint.Y <= halfExtents.Y)
            {
                // Use vector from rectangle center to point (in world space), normalized.
                Vector2 normal = NormalizeZeroSafe(pointWorld - pRectangleOriented.WorldPosition);
                return new CollisionEvent(true, pointWorld, normal);
            }

            return new CollisionEvent(false);
        }
    }
}
