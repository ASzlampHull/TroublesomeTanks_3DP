using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tankontroller.Managers;

namespace Tankontroller.World.Pickups
{
    public class MinePickup : Pickup
    {
        private static readonly Texture2D mMineTexture = Tankontroller.Instance().CM().Load<Texture2D>("MinePickup");

        public MinePickup(Vector2 pPositon) : base(mMineTexture, new Rectangle(400, 500, 40, 40), pPositon) { }

        public override bool PickUpCollision(Tank tank)
        {
            if (CollisionManager.Collide(tank, PickupRect, false))
            {
                tank.SetBulletType(BulletType.MINE);
                return true;
            }
            return false;
        }
    }
}
