using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.GUI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TTMapEditor.Managers;

namespace TTMapEditor.Scenes
{
    internal class MainMenuScene : IScene
    {
        IGame mGameInstance = TTMapEditor.Instance();

        ButtonList mButtonList = null;
        private static readonly Texture2D mForegroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("menu_white");
        private static readonly Texture2D mBackgroundTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("background_01");
        private static readonly Texture2D mTitleTexture = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("menu_title");
        Rectangle mBackgroundRectangle;
        Rectangle mTitleRectangle;
        Rectangle mControllerInfoRect;

        public MainMenuScene()
        {
            //Todo make it scale correctly based on screen size
            mSpriteBatch = new SpriteBatch(mGameInstance.GetGraphicsDeviceManager().GraphicsDevice);
            int screenWidth = mGameInstance.GetGraphicsDeviceManager().PreferredBackBufferWidth;
            int screenHeight = mGameInstance.GetGraphicsDeviceManager().PreferredBackBufferHeight;

            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            mTitleRectangle = new Rectangle((screenWidth / 2) - (644 / 2), (screenHeight / 2) - screenHeight / 3, 644 , 128);

            mControllerInfoRect = new Rectangle(0, 0, screenWidth / 5, screenHeight);

            mButtonList = new ButtonList();

            Texture2D startButtonTexture = mGameInstance.GetContentManager().Load<Texture2D>("Load_Map_Button");
            Texture2D exitButtonTexture = mGameInstance.GetContentManager().Load<Texture2D>("Exit_Button");
            Texture2D newMapButtonTexture = mGameInstance.GetContentManager().Load<Texture2D>("New_Map_Button");

            int buttonWidth = startButtonTexture.Width;
            int buttonHeight = startButtonTexture.Height;
            int buttonY = (screenHeight) / 4 + buttonHeight;
            int buttonX = (screenWidth - buttonWidth) / 2;
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            Button loadMapButton = new Button(startButtonTexture, startButtonTexture, buttonRect, Color.Yellow, LoadMap);
            loadMapButton.mSelected = true;
            mButtonList.AddButton(loadMapButton);

            buttonRect.Y += (int)(buttonHeight * 1.25f);
            Button newMapButton = new Button(newMapButtonTexture, newMapButtonTexture, buttonRect, Color.Yellow, NewMap);
            newMapButton.mSelected = false;
            mButtonList.AddButton(newMapButton);

            buttonRect.Y += (int)(buttonHeight * 1.25f);
            Button exitButton = new Button(exitButtonTexture, exitButtonTexture, buttonRect, Color.Yellow, ExitGame);
            exitButton.mSelected = false;
            mButtonList.AddButton(exitButton);

        }

        public void LoadMap()
        {
            mGameInstance.GetSceneManager().Transition(new MapSelectionScene(this), false);
        }

        public void NewMap()
        {
            string newMapName = MapManager.createNewMap("New_Map");
            mGameInstance.GetSceneManager().Transition(new MapEditingScene(this, newMapName, true), false);
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GetGraphicsDeviceManager().GraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            Color backColour = Color.White;

            mSpriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, backColour);
            mSpriteBatch.Draw(mForegroundTexture, mBackgroundRectangle, backColour);
            mSpriteBatch.Draw(mTitleTexture, mTitleRectangle, backColour);
            mButtonList.Draw(mSpriteBatch);
            mSpriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            NavigateMenu();
            InputManager.Update();
        }

        private void ExitGame()
        {
            mGameInstance.GetSceneManager().Transition(null);
        }

        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ExitGame();
            }
        }

        private void NavigateMenu()
        {
            if(InputManager.isKeyPressed(Keys.Down))
            {
                mButtonList.SelectNextButton();
            }
            else if(InputManager.isKeyPressed(Keys.Up))
            {
                mButtonList.SelectPreviousButton();
            }
            else if(InputManager.isKeyPressed(Keys.Enter))
            {
                mButtonList.PressSelectedButton();
            }
        }
    }
}
