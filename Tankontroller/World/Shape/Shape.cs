using Microsoft.Xna.Framework;
using Tankontroller.World.WorldObject;
using Tankontroller.Managers;

namespace Tankontroller.World.Shape
{
    internal abstract class Shape
    {
        private bool mEnabled = true;

        /// <summary>
        /// Owner of the collision shape.
        /// </summary>
        public Transform Owner { get; set; } = null;

        /// <summary>
        /// If false, shape is ignored by collision queries.
        /// </summary>
        public bool Enabled {
            get => mEnabled && Owner != null;
            set => mEnabled = value;
        }

        /// <summary>
        /// Local offset applied to the owner's position (or absolute position if Owner is null).
        /// </summary>
        public Vector2 LocalOffset { get; set; } = Vector2.Zero;

        /// <summary>
        /// World position of the collision shape (owner's position plus local offset).
        /// Gets local offset of owner is null.
        /// </summary>
        public Vector2 WorldPosition => Owner != null ? Owner.Position + LocalOffset : LocalOffset;

        public Shape(Transform pOwner, bool pEnabled = true)
        {
            Owner = pOwner;
            mEnabled = pEnabled;
        }
        
        public Shape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true)
        {
            Owner = pOwner;
            LocalOffset = pLocalOffset;
            mEnabled = pEnabled;
        }

        public abstract CollisionEvent Intersects(Shape other);
    }
}
