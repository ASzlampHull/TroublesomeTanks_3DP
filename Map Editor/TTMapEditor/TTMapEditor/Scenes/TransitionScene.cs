using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Scenes
{
    internal class TransitionScene : IScene
    {
        GraphicsDevice mGraphicsDevice;
        RenderTarget2D mPreviousTexture = null;
        RenderTarget2D mNextTexture = null;
        Rectangle mRectangle;
        IScene mNextScene;
        Vector2 mNextPosition = new Vector2(0, -(TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height));
        Vector2 mVelocity = new Vector2(0, 0);
        Vector2 mAcceleration = new Vector2(0, 1);

        public TransitionScene(IScene pPreviousScene, IScene pNextScene)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mNextScene = pNextScene;
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mRectangle = new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            mPreviousTexture = GenerateSceneTexture(pPreviousScene);
            mNextTexture = GenerateSceneTexture(pNextScene);
        }

        public RenderTarget2D GenerateSceneTexture(IScene pScene)
        {
            RenderTarget2D output = new RenderTarget2D(mGraphicsDevice, mGraphicsDevice.PresentationParameters.BackBufferWidth, mGraphicsDevice.PresentationParameters.BackBufferHeight, false, mGraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            mGraphicsDevice.SetRenderTarget(output);
            mGraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            mSpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            pScene.Draw(0);
            mSpriteBatch.End();
            mGraphicsDevice.SetRenderTarget(null);
            return output;
        }
        
        public override void Draw(float pSeconds)
        {
            mGraphicsDevice.Clear(Color.Black);
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mPreviousTexture, mRectangle, Color.White);
            mSpriteBatch.Draw(mNextTexture, mNextPosition, mRectangle, Color.White);
            mSpriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            IGame gameInstance = TTMapEditor.Instance();
            mVelocity += mAcceleration;
            mNextPosition += mVelocity;
            if (mNextPosition.Y > 0)
            {
                gameInstance.GetSceneManager().Pop();

                if(mNextScene != gameInstance.GetSceneManager().Top)
                {
                    gameInstance.GetSceneManager().Push(mNextScene);
                }
            }
        }
    }
}
