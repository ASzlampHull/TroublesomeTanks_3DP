using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class RectangleOrientedShape : CollisionShape
    {
        // Full size in local space (width, height)
        public Vector2 Size { get; set; } = Vector2.One;

        // Half extents in local space (width/2, height/2)
        public Vector2 HalfExtents => Size * 0.5f;

        // Local rotation relative to Owner.Rotation (radians)
        public float LocalRotation { get; set; } = 0f;

        // World rotation (radians)
        public float WorldRotation => Owner.Rotation + LocalRotation;

        // World-space axes (unit vectors)
        public Vector2 WorldAxisX
        {
            get
            {
                float cos = (float)Math.Cos(WorldRotation);
                float sin = (float)Math.Sin(WorldRotation);
                return new Vector2(cos, sin);
            }
        }

        public Vector2 WorldAxisY
        {
            get
            {
                float cos = (float)Math.Cos(WorldRotation);
                float sin = (float)Math.Sin(WorldRotation);
                return new Vector2(-sin, cos);
            }
        }

        public RectangleOrientedShape(Transform pOwner, Vector2 pSize, bool pEnabled = true) : base(pOwner, pEnabled)
        {
            Size = pSize;
        }

        public RectangleOrientedShape(Transform pOwner, Vector2 pSize, float pLocalRotation, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
            Size = pSize;
            LocalRotation = pLocalRotation;
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
        /// Checks for intersection with a point - if the point is inside the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (the point itself).
        /// 2. The normal of the collision (pointing into the rectangle). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            CollisionEvent collisionEvent = pPoint.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with circle - if the circle overlaps with the rectangle.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the circle and rectangle)
        /// 2. The normal of the collision (pointing into the rectangle) </returns>
        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            CollisionEvent collisionEvent = pCircle.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        /// <summary>
        /// Check for intersection with an axis aligned rectangle shape - if the rectangles overlap.
        /// </summary>
        /// <returns> Collision event information. If colliding:
        /// 1. The position of the collision (midpoint of overlap between the rectangles)
        /// 2. The normal of the collision (pointing away from the axis aligned rectangle) </returns>
        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            CollisionEvent collisionEvent = pRectangleAligned.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            // Build local axes for both rectangles in world space
            float cosA = (float)Math.Cos(WorldRotation);
            float sinA = (float)Math.Sin(WorldRotation);
            Vector2 axisA_X = new(cosA, sinA);
            Vector2 axisA_Y = new(-sinA, cosA);

            float cosB = (float)Math.Cos(pRectangleOriented.WorldRotation);
            float sinB = (float)Math.Sin(pRectangleOriented.WorldRotation);
            Vector2 axisB_X = new(cosB, sinB);
            Vector2 axisB_Y = new(-sinB, cosB);

            Vector2 centerA = WorldPosition;
            Vector2 centerB = pRectangleOriented.WorldPosition;

            Vector2 halfA = HalfExtents;
            Vector2 halfB = pRectangleOriented.HalfExtents;

            // Candidate axes: face normals of both rectangles
            Vector2[] axes = { axisA_X, axisA_Y, axisB_X, axisB_Y };

            float minOverlap = float.MaxValue;
            Vector2 minAxis = Vector2.Zero;

            foreach (Vector2 axis in axes)
            {
                // Project centers onto axis
                float projCenterA = Vector2.Dot(centerA, axis);
                float projCenterB = Vector2.Dot(centerB, axis);

                // Projected half extents for each rectangle onto axis
                float projHalfA = halfA.X * MathF.Abs(Vector2.Dot(axisA_X, axis)) + halfA.Y * MathF.Abs(Vector2.Dot(axisA_Y, axis));
                float projHalfB = halfB.X * MathF.Abs(Vector2.Dot(axisB_X, axis)) + halfB.Y * MathF.Abs(Vector2.Dot(axisB_Y, axis));

                float distance = MathF.Abs(projCenterA - projCenterB);
                float overlap = projHalfA + projHalfB - distance;

                // Separating axis found -> no collision
                if (overlap <= 0f)
                    return new CollisionEvent(false);

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    minAxis = axis;
                }
            }

            // Determine normal direction so it consistently points from the other rectangle to this rectangle
            float sign = MathF.Sign(Vector2.Dot(centerA - centerB, minAxis));
            if (sign == 0f) sign = 1f;
            Vector2 collisionNormal = NormalizeZeroSafe(minAxis * sign);

            // Support points: pick rectangle-support points in the direction of the collision normal, then midpoint
            float signAX = MathF.Sign(Vector2.Dot(collisionNormal, axisA_X));
            float signAY = MathF.Sign(Vector2.Dot(collisionNormal, axisA_Y));
            Vector2 supportA = centerA + axisA_X * (signAX * halfA.X) + axisA_Y * (signAY * halfA.Y);

            float signBX = MathF.Sign(Vector2.Dot(collisionNormal, axisB_X));
            float signBY = MathF.Sign(Vector2.Dot(collisionNormal, axisB_Y));
            Vector2 supportB = centerB + axisB_X * (signBX * halfB.X) + axisB_Y * (signBY * halfB.Y);

            Vector2 collisionPosition = (supportA + supportB) * 0.5f;

            return new CollisionEvent(true, collisionPosition, collisionNormal);
        }
    }
}
