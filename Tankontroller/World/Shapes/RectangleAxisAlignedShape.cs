using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class RectangleAxisAlignedShape : CollisionShape
    {
        public Vector2 Size { get; set; } = Vector2.One;
        public Vector2 HalfExtents => Size * 0.5f;
        public Vector2 Min => WorldPosition - HalfExtents;
        public Vector2 Max => WorldPosition + HalfExtents;

        public RectangleAxisAlignedShape(Transform pOwner, Vector2 pSize, bool pEnabled = true) : base(pOwner, pEnabled)
        {
            Size = pSize;
        }

        public RectangleAxisAlignedShape(Transform pOwner, Vector2 pSize, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
            Size = pSize;
        }

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
        /// Checks for intersection with a point shape - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing into the rectangle center). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            CollisionEvent collisionEvent = pPoint.IntersectsAlignedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with an axis-aligned rectangle shape - if the circle overlaps with the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the circle and rectangle)
        /// 2. The normal of the collision (pointing away from the rectangle) </returns>
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            CollisionEvent collisionEvent = pCircle.IntersectsAlignedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with another axis-aligned rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles)
        /// 2. The normal of the collision (pointing away from other rectangle) </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            Vector2 aMin = Min;
            Vector2 aMax = Max;
            Vector2 bMin = pRectangleAligned.Min;
            Vector2 bMax = pRectangleAligned.Max;

            float overlapX = MathF.Min(aMax.X, bMax.X) - MathF.Max(aMin.X, bMin.X);
            float overlapY = MathF.Min(aMax.Y, bMax.Y) - MathF.Max(aMin.Y, bMin.Y);

            // No overlap (use <= 0 to treat touching as non-colliding)
            if (overlapX <= 0f || overlapY <= 0f)
            {
                return new CollisionEvent(false);
            }

            // Intersection rectangle and midpoint
            Vector2 intersectionMin = new(MathF.Max(aMin.X, bMin.X), MathF.Max(aMin.Y, bMin.Y));
            Vector2 intersectionMax = new(MathF.Min(aMax.X, bMax.X), MathF.Min(aMax.Y, bMax.Y));
            Vector2 collisionPosition = (intersectionMin + intersectionMax) * 0.5f;

            // Choose axis of minimum penetration for the normal
            Vector2 direction = WorldPosition - pRectangleAligned.WorldPosition;
            Vector2 collisionNormal;
            if (overlapX < overlapY)
            {
                float sign = MathF.Sign(direction.X);
                collisionNormal = new Vector2(sign, 0f);
            }
            else
            {
                float sign = MathF.Sign(direction.Y);
                collisionNormal = new Vector2(0f, sign);
            }
            collisionNormal = NormalizeZeroSafe(collisionNormal);

            return new CollisionEvent(true, collisionPosition, collisionNormal);
        }

        /// <summary>
        /// Check for intersection with an oriented rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles)
        /// 2. The normal of the collision (pointing away from the oriented rectangle) </returns>
        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            // Build OBB local axes in world space
            float cos = (float)Math.Cos(pRectangleOriented.WorldRotation);
            float sin = (float)Math.Sin(pRectangleOriented.WorldRotation);
            Vector2 orientedRectangleXAxis = new(cos, sin);
            Vector2 orientedRectangleYAxis = new(-sin, cos);

            // AABB local axes in world space
            Vector2 alignedRectangleXAxis = new(1f, 0f);
            Vector2 alignedRectangleYAxis = new(0f, 1f);

            Vector2 alignedRectangleCenter = WorldPosition;
            Vector2 orientedRectangleCenter = pRectangleOriented.WorldPosition;

            Vector2 alignedRectangleHalfExtents = HalfExtents;
            Vector2 orientedRectangleHalfExtents = pRectangleOriented.HalfExtents;

            // Test axes
            Vector2[] axes = { alignedRectangleXAxis, alignedRectangleYAxis, orientedRectangleXAxis, orientedRectangleYAxis };

            float minOverlap = float.MaxValue;
            Vector2 minAxis = Vector2.Zero;

            foreach (Vector2 axis in axes)
            {
                // Project centers
                float projectedAlignedRectangleCenter = Vector2.Dot(alignedRectangleCenter, axis);
                float projectedOrientedRectangleCenter = Vector2.Dot(orientedRectangleCenter, axis);

                float projectionHalfExtentAlignedRectangle = alignedRectangleHalfExtents.X * MathF.Abs(axis.X) + alignedRectangleHalfExtents.Y * MathF.Abs(axis.Y);
                float projectionHalfExtentOrientedRectangle = orientedRectangleHalfExtents.X * MathF.Abs(Vector2.Dot(orientedRectangleXAxis, axis)) + orientedRectangleHalfExtents.Y * MathF.Abs(Vector2.Dot(orientedRectangleYAxis, axis));

                float distanceBetweenProjectedCenters = MathF.Abs(projectedOrientedRectangleCenter - projectedAlignedRectangleCenter);
                float overlap = projectionHalfExtentAlignedRectangle + projectionHalfExtentOrientedRectangle - distanceBetweenProjectedCenters;

                // No overlap -> separating axis found
                if (overlap <= 0f)
                    return new CollisionEvent(false);

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    minAxis = axis;
                }
            }

            // Determine normal direction
            float sign = MathF.Sign(Vector2.Dot(alignedRectangleCenter - orientedRectangleCenter, minAxis));
            if (sign == 0f) sign = 1f;
            Vector2 collisionNormal = NormalizeZeroSafe(minAxis * sign);

            // Approximate contact point: use support points on each rect in direction of the normal,
            // then take their midpoint as an approximate collision position.
            float alignedRectangleSignX = MathF.Sign(Vector2.Dot(collisionNormal, alignedRectangleXAxis));
            float alignedRectangleSignY = MathF.Sign(Vector2.Dot(collisionNormal, alignedRectangleYAxis));
            Vector2 supportA = alignedRectangleCenter + new Vector2(alignedRectangleSignX * alignedRectangleHalfExtents.X, alignedRectangleSignY * alignedRectangleHalfExtents.Y);

            float orientedRectangleSignX = MathF.Sign(Vector2.Dot(collisionNormal, orientedRectangleXAxis));
            float orientedRectangleSignY = MathF.Sign(Vector2.Dot(collisionNormal, orientedRectangleYAxis));
            Vector2 supportB = orientedRectangleCenter + orientedRectangleXAxis * (orientedRectangleSignX * orientedRectangleHalfExtents.X) + orientedRectangleYAxis * (orientedRectangleSignY * orientedRectangleHalfExtents.Y);

            Vector2 collisionPosition = (supportA + supportB) * 0.5f;

            return new CollisionEvent(true, collisionPosition, collisionNormal);
        }
    }
}
