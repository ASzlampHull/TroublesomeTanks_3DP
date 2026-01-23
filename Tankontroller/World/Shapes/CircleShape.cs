using Microsoft.Xna.Framework;
using System;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class CircleShape : Shape
    {
        public CircleShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled)
        {

        }

        public CircleShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {

        }

        public override CollisionEvent Intersects(Shape other)
        {

            return new CollisionEvent(false);
        }
    }
}
