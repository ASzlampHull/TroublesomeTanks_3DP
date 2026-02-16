using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.World.Shapes;

namespace Tankontroller.World.WorldObject
{
    public interface IWorldObject
    {
        Transform Transform { get; }
        CollisionShape CollisionShape { get; }
    }
}
