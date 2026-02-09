using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class BouncyBulletPickup : Pickup
    {
        private static readonly Texture2D mBouncyBulletTexture = Tankontroller.Instance().CM().Load<Texture2D>("BouncyBulletPickup");

        public BouncyBulletPickup(Vector2 pPositon) : base(mBouncyBulletTexture, new Rectangle(400, 500, 40, 40), pPositon) { }

        public override bool PickUpCollision(Tank tank)
        {
            if (CollisionManager.Collide(tank, PickupRect, false))
            {
                tank.SetBulletType(BulletType.BOUNCY_BULLET);
                return true;
            }
            return false;
        }
    }
}
