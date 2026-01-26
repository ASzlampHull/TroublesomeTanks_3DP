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
            collisionEvent.CollisionNormal *= -1;
            return collisionEvent;
        }

        public CollisionEvent IntersectsCircle(CircleShape pCircle)
        {
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
