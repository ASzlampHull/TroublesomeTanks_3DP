using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal class RectangleAxisAlignedShape : Shape
    {
        public RectangleAxisAlignedShape(Transform pOwner, bool pEnabled = true) : base(pOwner, pEnabled)
        {
        }

        public RectangleAxisAlignedShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true) : base(pOwner, pLocalOffset, pEnabled)
        {
        }

        public override CollisionEvent Intersects(Shape other)
        {
            throw new NotImplementedException();
        }
    }
}
