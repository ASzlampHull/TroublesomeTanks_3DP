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
        public RectangleOrientedShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled)
        {
        }

        public RectangleOrientedShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
        }

        public override CollisionEvent Intersects(CollisionShape other)
        {
            throw new NotImplementedException();
        }
    }
}
