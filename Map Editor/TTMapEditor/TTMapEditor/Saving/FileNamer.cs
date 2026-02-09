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
        bool mIsActive = false;


        public void Update(float pDeltaTime)
        {
            if(!mIsActive)
            {
                return;
            }
            mTimeSinceLastKeyPress += pDeltaTime;
            foreach(Keys k in Keyboard.GetState().GetPressedKeys())
            {
                if(mTimeSinceLastKeyPress < mTimeBetweenKeyPresses)
                {
                    continue;
                }
                if(k == Keys.Back && mCurrentName.Length > 0)
                {
                    mCurrentName = mCurrentName.Substring(0, mCurrentName.Length - 1);
                    mTimeSinceLastKeyPress = 0f;
                    continue;
                }
                mCurrentName += k.ToString();
                mTimeSinceLastKeyPress = 0f;

            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            if (!mIsActive)
            {
                return;
            }
            pSpriteBatch.DrawString(mFont, mCurrentName, new Vector2(100, 100), Color.White);
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
