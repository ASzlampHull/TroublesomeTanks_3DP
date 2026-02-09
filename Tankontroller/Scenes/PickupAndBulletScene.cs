using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tankontroller.Controller;
using Tankontroller.GUI;
using static System.Formats.Asn1.AsnWriter;

namespace Tankontroller.Scenes
{
    public class PickupAndBulletScene : IScene
    {
        private static readonly Texture2D mBackgroundTexture = Tankontroller.Instance().CM().Load<Texture2D>("background_01");
        private Rectangle mBackgroundRectangle;
        private Rectangle mPickupinfoRectangle;
        private Tankontroller mGameInstance;
        private MainMenuScene mStartScene;
        private Texture2D mContinueButtonTexture;
        private Rectangle mContinueButtonRectangle;
        private Texture2D mContinueTextTexture;
        private Rectangle mContinueTextRectangle;

        private ButtonList mButtonList = null;

        public PickupAndBulletScene(MainMenuScene startScene)
        {
            mStartScene = startScene;
            mGameInstance = (Tankontroller)Tankontroller.Instance();
            spriteBatch = new SpriteBatch(mGameInstance.GDM().GraphicsDevice);
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;
            Tankontroller game = (Tankontroller)Tankontroller.Instance();
            mBackgroundRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            mContinueButtonTexture = game.CM().Load<Texture2D>("fire");
            mContinueButtonRectangle = new Rectangle(10, screenHeight / 2, mContinueButtonTexture.Width / 2, mContinueButtonTexture.Height / 2);
            mContinueTextTexture = game.CM().Load<Texture2D>("back");
            mContinueTextRectangle = new Rectangle(20 + mContinueButtonTexture.Width / 2, screenHeight / 2 + mContinueButtonTexture.Height / 4, mContinueTextTexture.Width, mContinueTextTexture.Height);

            GenerateButtons();
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            mButtonList.Draw(spriteBatch);
            spriteBatch.Draw(mContinueButtonTexture, mContinueButtonRectangle, Color.White);
            spriteBatch.Draw(mContinueTextTexture, mContinueTextRectangle, Color.White);
            spriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            mGameInstance.GetControllerManager().DetectControllers();

            foreach (IController controller in mGameInstance.GetControllerManager().GetControllers())
            {
                controller.UpdateController();
                if (controller.IsPressed(Control.FIRE))
                {
                    IGame game = Tankontroller.Instance();
                    game.GetControllerManager().SetAllTheLEDsWhite();
                    game.SM().Transition(null);
                }
            }
        }
        public override void Escape()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                mGameInstance.SM().Transition(mStartScene, true);
            }
        }

        /// <summary>
        /// ONLY FOR DEBUGGING PURPOSES, THIS FUNCTION DOES NOT DO ANYTHING AND IS ONLY USED TO TEST THE FUNCTION OF THE BUTTONS
        /// </summary>
        void DUMMYFUNCTION()
            { }

        /// <summary>
        /// Creates the pickup and frequency buttons for the scene and assigns their actions.
        /// </summary>
        private void GenerateButtons()
        {
            float scaleFactor = Tankontroller.Instance().ScaleFactor();
            int screenWidth = mGameInstance.GDM().GraphicsDevice.Viewport.Width;
            int screenHeight = mGameInstance.GDM().GraphicsDevice.Viewport.Height;

            mButtonList = new ButtonList();

            Texture2D pickupSelectionOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_off");
            Texture2D pickupSelectionOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_off_highlight");

            int buttonWidth = (int)(pickupSelectionOffTexture.Width * Tankontroller.Instance().ScaleFactor());
            int buttonHeight = (int)(pickupSelectionOffTexture.Height * Tankontroller.Instance().ScaleFactor());
            int buttonX = (int)(366 * scaleFactor);
            int buttonY = (int)(54 * scaleFactor);
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            // Selection "None" button
            Button selectionNoneButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionNoneButton.Selected = true;
            mButtonList.Add(selectionNoneButton);

            // Selection "Low" button
            buttonRect.X += buttonWidth;
            Button selectionLowButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionLowButton.Selected = false;
            mButtonList.Add(selectionLowButton);

            // Selection "Mid" button
            buttonRect.X += buttonWidth;
            Button selectionMidButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionMidButton.Selected = false;
            mButtonList.Add(selectionMidButton);

            // Selection "High" button
            buttonRect.X += buttonWidth;
            Button selectionHighButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionHighButton.Selected = false;
            mButtonList.Add(selectionHighButton);

            Texture2D pickupHealthOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_health_off");
            Texture2D pickupHealthOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_health_off_highlight");
            Texture2D pickupBallOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_ball_off");
            Texture2D pickupBallOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_ball_off_highlight");
            Texture2D pickupEMPOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_emp_off");
            Texture2D pickupEMPOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_emp_off_highlight");
            Texture2D pickupMineOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_mine_off");
            Texture2D pickupMineOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_mine_off_highlight");

            buttonWidth = (int)(pickupBallOffTexture.Width * scaleFactor);
            buttonHeight = (int)(pickupBallOffTexture.Height * scaleFactor);
            buttonX = (int)(428 * scaleFactor);
            buttonY = (int)(208 * scaleFactor);
            buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

            // Health Pickup button
            Button pickupHealthButton = new Button(pickupHealthOffTexture, pickupHealthOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupHealthButton.Selected = false;
            mButtonList.Add(pickupHealthButton);

            // Ball Pickup button
            buttonRect.Y += buttonHeight;
            Button pickupBallButton = new Button(pickupBallOffTexture, pickupBallOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupBallButton.Selected = false;
            mButtonList.Add(pickupBallButton);

            // EMP Pickup button
            buttonRect.Y += buttonHeight;
            Button pickupEMPButton = new Button(pickupEMPOffTexture, pickupEMPOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupEMPButton.Selected = false;
            mButtonList.Add(pickupEMPButton);

            // Mine Pickup button
            buttonRect.Y += buttonHeight;
            Button pickupMineButton = new Button(pickupMineOffTexture, pickupMineOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupMineButton.Selected = false;
            mButtonList.Add(pickupMineButton);
        }
    }
}
