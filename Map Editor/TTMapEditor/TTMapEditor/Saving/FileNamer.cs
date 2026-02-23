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
            bool capsLock = Keyboard.GetState().CapsLock;

            if (!mIsActive)
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

            Keys key = pressedKeys[0];

            if (!IsValidKey(key))
            {
                return;
            }

            if (key != mPreviousKey)
            {
                mTimeSinceLastKeyPress = mTimeBetweenKeyPresses;
            }


            else if (mTimeSinceLastKeyPress < mTimeBetweenKeyPresses)
            {
                return;
            }

            if (key == Keys.Back && mCurrentName.Length > 0)
            {
                mCurrentName = mCurrentName.Substring(0, mCurrentName.Length - 1);
                mTimeSinceLastKeyPress = 0f;
                mPreviousKey = key;
                return;
            }

            if (mFont.MeasureString(mCurrentName).X > mTypingRectangle.Width - 20)
            {
                return;
            }

            if (key == Keys.Space)
            {
                mCurrentName += " ";
                mPreviousKey = pressedKeys[0];
                mTimeSinceLastKeyPress = 0f;
                return;
            }

            char? ch = KeyToChar(key, capsLock);
            if (ch.HasValue)
            {
                mCurrentName += ch.Value;
            }
            mPreviousKey = key;
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

        static bool IsValidKey(Keys pKey)
        {
            return (pKey >= Keys.A && pKey <= Keys.Z) || (pKey >= Keys.D0 && pKey <= Keys.D9) || pKey == Keys.Space || pKey == Keys.Back;
        }

        static char? KeyToChar(Keys pKey, bool pCapsLock)
        {
            // Letters A–Z
            if (pKey >= Keys.A && pKey <= Keys.Z)
            {
                char c = (char)('A' + (pKey - Keys.A));
                return pCapsLock ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
            }

            // Top-row digits D0–D9 -> '0'–'9'
            if (pKey >= Keys.D0 && pKey <= Keys.D9)
            {
                return (char)('0' + (pKey - Keys.D0));
            }

            // Numpad digits -> '0'–'9'
            if (pKey >= Keys.NumPad0 && pKey <= Keys.NumPad9)
            {
                return (char)('0' + (pKey - Keys.NumPad0));
            }

            // Space handled earlier, Backspace returns null (no char)
            return null;
        }
    }
}
