using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using TTMapEditor.Managers;

namespace TTMapEditor.Saving
{
    public class FolderPicker
    {

        readonly SpriteBatch mSpriteBatch;
        readonly Texture2D mPixel;
        readonly SpriteFont mFont;
        string mCurrentPath;
        bool mTypingNewName = true;
        string mNewFolderName = "";
        Rectangle mOverlay;
        float mTypingTimer = 0f;
        float mTypingDelay = 0.1f;


        public FolderPicker(SpriteBatch pSpriteBatch, Texture2D pPixel, SpriteFont pFont, string pStartingPath)
        {
            mSpriteBatch = pSpriteBatch;
            mPixel = pPixel;
            mFont = pFont;
            mCurrentPath = pStartingPath;
            mOverlay = new Rectangle(0, 0, 800, 600);
            mFont = TTMapEditor.Instance().GetContentManager().Load<SpriteFont>("FolderPickerText");
        }

        public bool createNewFolder()
        {
            return false;
        }

        public void Update(float pDeltaTime)
        {
            mTypingTimer += pDeltaTime;
            if (mTypingNewName)
            {
                foreach(Keys k in Enum.GetValues(typeof(Keys)))
                {
                    if (InputManager.isKeyDown(k) && mTypingTimer >= mTypingDelay)
                    {
                        if(k == Keys.Space)
                        {
                            mNewFolderName += " ";
                            mTypingTimer = 0f;
                            continue;
                        }
                        if(k == Keys.Back && mNewFolderName.Length > 0)
                        {
                            mNewFolderName = mNewFolderName.Substring(0, mNewFolderName.Length - 1);
                            mTypingTimer = 0f;
                            continue;
                        }
                        if(k == Keys.Enter)
                        {
                            mTypingNewName = false;
                            createNewFolder();
                            mTypingTimer = 0f;
                            continue;
                        }
                        string s = k.ToString();
                        if (s.Length == 1)
                        {
                            mNewFolderName += s;
                            if(mFont.MeasureString(mNewFolderName).X > 780)
                            {
                                mNewFolderName = mNewFolderName.Substring(0, mNewFolderName.Length - 1);
                                mNewFolderName += "\n";
                                mNewFolderName += s;
                            }
                        }
                        mTypingTimer = 0f;
                    }
                }
            }
        }

        public void Draw()
        {
            mSpriteBatch.Begin();
            mSpriteBatch.Draw(mPixel, mOverlay, Color.Black);
            mSpriteBatch.DrawString(mFont, "Current Path: " + mCurrentPath, new Vector2(10, 10), Color.White);
            mSpriteBatch.DrawString(mFont, mNewFolderName, new Vector2(10, 30), Color.White);
            mSpriteBatch.End();
        }
    
    
    }
}
