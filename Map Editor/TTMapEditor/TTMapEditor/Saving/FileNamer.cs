using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TTMapEditor.Saving
{
    public class FileNamer
    {
        private string mCurrentName;
        float mTimeBetweenKeyPresses = 0.2f;
        float mTimeSinceLastKeyPress = 0f;
        private static readonly SpriteFont mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("FolderPickerText");
        Rectangle mTypingRectangle;
        Texture2D mTypingBackground = TTMapEditor.Instance().GetContentManager().Load<Texture2D>("block");
        bool mIsActive = false;
        Keys mPreviousKey;
        Keys mCurrentKey;

        public FileNamer()
        {
            mCurrentName = "";
            mTypingRectangle = new Rectangle(100, 100, 400, 50);
        }

        public void Update(float pDeltaTime)
        {
            if(!mIsActive)
            {
                return;
            }

            mTimeSinceLastKeyPress += pDeltaTime;
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            int numPressedKeys = pressedKeys.Length;
            if (numPressedKeys == 0)
            {
                mTimeSinceLastKeyPress = mTimeBetweenKeyPresses;
                return;
            }

            if (pressedKeys[0] == Keys.Enter)
            {
                return;
            }
                
            if (pressedKeys[0] != mPreviousKey)               
            {                    
                mTimeSinceLastKeyPress = mTimeBetweenKeyPresses;               
            }

                
            else if (mTimeSinceLastKeyPress < mTimeBetweenKeyPresses)               
            {
                return;              
            }
   
            if (pressedKeys[0] == Keys.Back && mCurrentName.Length > 0)                
            {                    
                mCurrentName = mCurrentName.Substring(0, mCurrentName.Length - 1);                    
                mTimeSinceLastKeyPress = 0f;                   
                mPreviousKey = pressedKeys[0];
                return;
            }

            if (mFont.MeasureString(mCurrentName).X > mTypingRectangle.Width - 20)
            {
                return;
            }

            if (pressedKeys[0] == Keys.Space)
            {
                mCurrentName += " ";
                mPreviousKey = pressedKeys[0];
                mTimeSinceLastKeyPress = 0f;
                return;
            }

            mCurrentName += pressedKeys[0].ToString();                
            mPreviousKey = pressedKeys[0];   
            mTimeSinceLastKeyPress = 0f;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            if (!mIsActive)
            {
                return;
            }
            pSpriteBatch.Draw(mTypingBackground, mTypingRectangle, Color.Black);
            pSpriteBatch.DrawString(mFont, "Enter name:", new Vector2(100, 100), Color.White);
            pSpriteBatch.DrawString(mFont, mCurrentName, new Vector2(100, 115), Color.White);
            pSpriteBatch.DrawString(mFont, "Press Enter to confirm", new Vector2(100, 130), Color.White);
        }

        public string ReturnName()
        {
            mIsActive = false;
            return mCurrentName;
        }

        public void StartTyping()
        {
            mIsActive = true;
            mCurrentName = "";
        }

        public bool IsActive()
        {
            return mIsActive;
        }


    }
}
