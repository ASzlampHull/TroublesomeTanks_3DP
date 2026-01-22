using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TTMapEditor.Objects
{
    internal class Tank : SceneObject
    {
        private static readonly Color COLOUR = Color.Blue;

        public Tank(Texture2D pTexture, Rectangle pRectangle) : base(pTexture, pRectangle) { }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // highlight when selected
            pSpriteBatch.Draw(mTexture, mRectangle, GetIsSelected() ? Color.Yellow : COLOUR);
        }

        public override void DrawOutline(SpriteBatch pSpriteBatch) => pSpriteBatch.Draw(mTexture, mOutlineRectangle, Color.Black);
    }
}
