using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class EMPPickup : Pickup
    {
        private static readonly Texture2D mEMPTexture = Tankontroller.Instance().CM().Load<Texture2D>("EMP");

        public EMPPickup(Vector2 pPositon) : base(mEMPTexture, new Rectangle(400, 500, 40, 40), pPositon) { }

        public override bool PickUpCollision(Tank tank)
        {
            if (CollisionManager.Collide(tank, PickupRect, false))
            {
                tank.SetBulletType(BulletType.BOUNCY_EMP);
                return true;
            }
            return false;
        }
    }
}
