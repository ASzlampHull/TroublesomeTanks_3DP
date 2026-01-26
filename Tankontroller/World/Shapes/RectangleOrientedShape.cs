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

        public CollisionEvent IntersectsPoint(PointShape pPoint)
        {
            return new CollisionEvent(false);
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
