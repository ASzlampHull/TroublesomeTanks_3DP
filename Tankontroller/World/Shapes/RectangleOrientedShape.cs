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
        /// 2. The normal of the collision (pointing into the rectangle center). </returns>
        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            CollisionEvent collisionEvent = pPoint.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
            CollisionEvent collisionEvent = pCircle.IntersectsOrientedRectangle(this);
            if (collisionEvent.CollisionNormal.HasValue)
                collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        public CollisionEvent IntersectsAlignedRectangle(RectangleAxisAlignedShape pRectangleAligned)
        {
            throw new NotImplementedException($"Intersection with shape {this} and {pRectangleAligned} is not implemented.");
        }

        public CollisionEvent IntersectsOrientedRectangle(RectangleOrientedShape pRectangleOriented)
        {
            throw new NotImplementedException($"Intersection with shape {this} and {pRectangleOriented} is not implemented.");
        }
    }
}
