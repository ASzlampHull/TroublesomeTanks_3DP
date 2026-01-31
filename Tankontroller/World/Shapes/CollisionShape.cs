using Microsoft.Xna.Framework;
using Tankontroller.World.WorldObject;

namespace Tankontroller.World.Shapes
{
    internal abstract class CollisionShape
    {
        private bool mEnabled = true;

        /// <summary>
        /// Owner of the collision shape.
        /// </summary>
        public Transform Owner { get; set; } = null;

        /// <summary>
        /// If false, shape is ignored by collision queries.
        /// Must have an owner and mEnabled flag must be true.
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

        public CollisionShape(Transform pOwner, bool pEnabled = true)
        {
            Owner = pOwner;
            mEnabled = pEnabled;
        }
        
        public CollisionShape(Transform pOwner, Vector2 pLocalOffset, bool pEnabled = true)
        {
            Owner = pOwner;
            LocalOffset = pLocalOffset;
            mEnabled = pEnabled;
        }

        /// <summary>
        /// Checks for intersection with another shape (enables polymorphic behavior).
        /// </summary>
        /// <returns> Collision event information, including whether a collision has occurred </returns>
        /// <exception cref="NotImplementedException"> Thrown when intersection with an unsupported shape is attempted. </exception>
        public abstract CollisionEvent Intersects(CollisionShape other);


        /// <summary>
        /// Normalizes a vector, returning a default unit vector if the input vector is zero-length.
        /// </summary>
        /// <returns> Normalized vector or 1,0 vector if zero </returns>
        protected static Vector2 NormalizeZeroSafe(Vector2 vector)
        {
            if (vector.LengthSquared() <= 0f)
            {
                return new Vector2(1f, 0f);
            }
            return Vector2.Normalize(vector);
        }
    }
}
