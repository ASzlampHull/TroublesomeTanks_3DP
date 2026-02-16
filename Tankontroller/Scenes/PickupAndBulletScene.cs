using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        private static readonly SpriteFont m_SpriteFont = Tankontroller.Instance().CM().Load<SpriteFont>("handwritingfont");
        private Rectangle mBackgroundRectangle;
        private Rectangle mPickupinfoRectangle;
        private Tankontroller mGameInstance;
        private MainMenuScene mStartScene;
        private Texture2D mContinueButtonTexture;
        private Rectangle mContinueButtonRectangle;
        private Texture2D mContinueTextTexture;
        private Rectangle mContinueTextRectangle;

        private ButtonList mButtonList = null;
        private TextList mTextList = null;

        // Cooldown timers to prevent the menu selection from being too fast
        private float mSelectionCooldown = 0.0f;
        private readonly float SELECTION_COOLDOWN_TIME = 0.2f;

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
            mSelectionCooldown = SELECTION_COOLDOWN_TIME;

            GenerateButtons();
        }

        public override void Draw(float pSeconds)
        {
            mGameInstance.GDM().GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mBackgroundTexture, mBackgroundRectangle, Color.White);
            mButtonList.Draw(spriteBatch);
            mTextList.Draw(spriteBatch);
            spriteBatch.Draw(mContinueButtonTexture, mContinueButtonRectangle, Color.White);
            spriteBatch.Draw(mContinueTextTexture, mContinueTextRectangle, Color.White);
            spriteBatch.End();
        }

        public override void Update(float pSeconds)
        {
            Escape();
            mGameInstance.GetControllerManager().DetectControllers();

            mSelectionCooldown -= pSeconds;

            foreach (IController controller in mGameInstance.GetControllerManager().GetControllers())
            {
                controller.UpdateController();

                if (controller.IsPressed(Control.TURRET_LEFT) && mSelectionCooldown <= 0.0f)
                {
                    mButtonList.SelectPreviousButton();
                    mSelectionCooldown = SELECTION_COOLDOWN_TIME;
                }
                if (controller.IsPressed(Control.TURRET_RIGHT) && mSelectionCooldown <= 0.0f)
                {
                    mButtonList.SelectNextButton();
                    mSelectionCooldown = SELECTION_COOLDOWN_TIME;
                }

                if (controller.IsPressed(Control.FIRE) && !controller.WasPressed(Control.FIRE) ||
                    controller.IsPressed(Control.RECHARGE) && !controller.WasPressed(Control.RECHARGE))
                {
                    SoundEffectInstance buttonPress = mGameInstance.GetSoundManager().GetSoundEffectInstance("Sounds/Button_Push");
                    buttonPress.Play();
                    mButtonList.PressSelectedButton();
                }

                //if (controller.IsPressed(Control.FIRE))
                //{
                //    IGame game = Tankontroller.Instance();
                //    game.GetControllerManager().SetAllTheLEDsWhite();
                //    game.SM().Transition(null);
                //}
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
            //y offset for 32:9 aspect ratio
            if ((float)screenWidth / (float)screenHeight == 32f / 9f)
            {
                scaleFactor = 1f;
            }

            mButtonList = new ButtonList();
            mTextList = new TextList();

            Texture2D pickupSelectionOffTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_off");
            Texture2D pickupSelectionOffHighlightTexture = mGameInstance.CM().Load<Texture2D>("PickupMenu/pickupinfo_selection_off_highlight");

            int buttonWidth = (int)(pickupSelectionOffTexture.Width * scaleFactor);
            int buttonHeight = (int)(pickupSelectionOffTexture.Height * scaleFactor);
            int buttonX = Convert.ToInt32((screenWidth - buttonWidth) / 5);
            int buttonY = (int)(screenHeight / 35.55);
            int buttonXPadding = (int)(screenWidth / 6);
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            int buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.5);
            int buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.5);
            Vector2 buttonTextPosition = new Vector2(buttonTextX, buttonTextY);

            // Selection "None" button
            Button selectionNoneButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionNoneButton.Selected = true;
            mButtonList.Add(selectionNoneButton);
            Text selectionNoneButtonText = new Text(m_SpriteFont, "None", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionNoneButtonText);

            // Selection "Low" button
            buttonRect.X += buttonXPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.3);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button selectionLowButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionLowButton.Selected = false;
            mButtonList.Add(selectionLowButton);
            Text selectionLowButtonText = new Text(m_SpriteFont, "Low", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionLowButtonText);

            // Selection "Med" button
            buttonRect.X += buttonXPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.4);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button selectionMedButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionMedButton.Selected = false;
            mButtonList.Add(selectionMedButton);
            Text selectionMedButtonText = new Text(m_SpriteFont, "Med", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionMedButtonText);

            // Selection "High" button
            buttonRect.X += buttonXPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 2.4);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button selectionHighButton = new Button(pickupSelectionOffTexture, pickupSelectionOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            selectionHighButton.Selected = false;
            mButtonList.Add(selectionHighButton);
            Text selectionHighButtonText = new Text(m_SpriteFont, "High", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(selectionHighButtonText);

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
            buttonX = Convert.ToInt32((screenWidth-buttonWidth) / 2);
            buttonY = (int)(screenHeight / 4.23);
            int buttonYPadding = (int)(screenHeight / 5.68);
            buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.5);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);

            // Health Pickup button
            Button pickupHealthButton = new Button(pickupHealthOffTexture, pickupHealthOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupHealthButton.Selected = false;
            mButtonList.Add(pickupHealthButton);
            Text pickupHealthButtonText = new Text(m_SpriteFont, "The Health - Heals the tank for 1 hp", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupHealthButtonText);

            // Ball Pickup button
            buttonRect.Y += buttonYPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 3.0);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button pickupBallButton = new Button(pickupBallOffTexture, pickupBallOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupBallButton.Selected = false;
            mButtonList.Add(pickupBallButton);
            Text pickupBallButtonText = new Text(m_SpriteFont, "The Bouncy - A bullet that can bounce off walls", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupBallButtonText);

            // EMP Pickup button
            buttonRect.Y += buttonYPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.7);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button pickupEMPButton = new Button(pickupEMPOffTexture, pickupEMPOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupEMPButton.Selected = false;
            mButtonList.Add(pickupEMPButton);
            Text pickupEMPButtonText = new Text(m_SpriteFont, "The EMP - Drains the energy of any tank in its shockwave", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupEMPButtonText);

            // Mine Pickup button
            buttonRect.Y += buttonYPadding;
            buttonTextX = Convert.ToInt32(buttonRect.X + buttonRect.Width / 3.8);
            buttonTextY = Convert.ToInt32(buttonRect.Y + buttonRect.Height / 2.45);
            buttonTextPosition = new Vector2(buttonTextX, buttonTextY);
            Button pickupMineButton = new Button(pickupMineOffTexture, pickupMineOffHighlightTexture, buttonRect, Color.Red, DUMMYFUNCTION);
            pickupMineButton.Selected = false;
            mButtonList.Add(pickupMineButton);
            Text pickupMineButtonText = new Text(m_SpriteFont, "The Mine - Damages the tank who drives over it", buttonTextPosition, Color.White, scaleFactor);
            mTextList.Add(pickupMineButtonText);
        }
    }
}
