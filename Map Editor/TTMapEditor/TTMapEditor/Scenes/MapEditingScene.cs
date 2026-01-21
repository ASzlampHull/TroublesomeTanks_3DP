using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.Managers;
using TTMapEditor.Maps;
using TTMapEditor.Objects;

namespace TTMapEditor.Scenes
{
    internal class MapEditingScene : IScene
    {

        GraphicsDevice mGraphicsDevice;
        IGame mGameInstance = TTMapEditor.Instance();
        MainMenuScene mStartScene;
        String mName;
        static readonly SpriteFont mTitleFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("TitleFont");
        static readonly Texture2D mBackgroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("background_01");
        static readonly Texture2D mPixelTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("white_pixel");
        Rectangle mBackgroundRectangle;
        Rectangle mPlayArea;
        Rectangle mPlayAreaOutline;
        MapPreview mPreview;
        bool mIsNewMap;

        public MapEditingScene(MainMenuScene pStartScene, string pMapFile, bool pIsNewMap)
        {
            mGraphicsDevice = TTMapEditor.Instance().GetGraphicsDeviceManager().GraphicsDevice;
            mStartScene = pStartScene;
            mName = pMapFile;
            mPreview = new MapPreview(pMapFile);
            mPlayArea = mPreview.GetPlayArea();
            mPlayAreaOutline = new Rectangle(mPlayArea.X - 5, mPlayArea.Y - 5, mPlayArea.Width + 10, mPlayArea.Height + 10);
            mSpriteBatch = new SpriteBatch(mGraphicsDevice);
            mBackgroundRectangle = new Rectangle(0,0, mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Width, mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height);
            mIsNewMap = pIsNewMap;
        }

        public override void Draw(float pSeconds)
        {
            
            mGraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            mSpriteBatch.DrawString(mTitleFont, mName, new Vector2(100, 100), Color.Black);
            mSpriteBatch.Draw(mPixelTexture, mPlayAreaOutline, Color.Black);
            mSpriteBatch.Draw(mPixelTexture,mPlayArea,Color.Wheat);
            if (!mIsNewMap)
            {
                foreach (RectWall wall in mPreview.GetWalls())
                {
                    wall.DrawOutline(mSpriteBatch);
                    wall.Draw(mSpriteBatch);
                }
            }


            mSpriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            InputManager.Update();
            if(InputManager.isKeyPressed(Keys.Escape))
            {
                mGameInstance.GetSceneManager().Transition(mStartScene);
            }

        }
    }
}
