using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace TTMapEditor.GUI
{
    internal class ButtonList
    {
        List<Button> mButtons = null;
        int currentSelectedButtonIndex = 0;

        public ButtonList()
        {
            mButtons = new List<Button>();
        }
        
        public void AddButton(Button pButton)
        {
            mButtons.Add(pButton);
        }

        public void SelectNextButton()
        {
            Console.WriteLine("NextButton start: " + currentSelectedButtonIndex);
            int nextSelectedButtonIndex = currentSelectedButtonIndex + 1;
            if (nextSelectedButtonIndex >= mButtons.Count)
            {
                nextSelectedButtonIndex = 0;
            }
            mButtons[nextSelectedButtonIndex].mSelected = true;
            mButtons[currentSelectedButtonIndex].mSelected = false;
            currentSelectedButtonIndex = nextSelectedButtonIndex;
            Console.WriteLine("NextButton finish: " + currentSelectedButtonIndex);
        }

        public void SelectPreviousButton()
        {
            Console.WriteLine("PreviousButton start: " + currentSelectedButtonIndex);
            int previousSelectedButtonIndex = currentSelectedButtonIndex - 1;
            if (previousSelectedButtonIndex < 0)
            {
                previousSelectedButtonIndex = mButtons.Count - 1;
            }
            mButtons[previousSelectedButtonIndex].mSelected = true;
            mButtons[currentSelectedButtonIndex].mSelected = false;
            currentSelectedButtonIndex = previousSelectedButtonIndex;
            Console.WriteLine("PreviousButton finish: " + currentSelectedButtonIndex);
        }

        public void PressSelectedButton()
        {
            mButtons[currentSelectedButtonIndex].PressButton();
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            foreach (Button button in mButtons)
            {
                Color buttonColour = Color.White;
                if (button.mSelected)
                    pSpriteBatch.Draw(button.mTexturePressed, button.mRectangle, Color.Lerp(buttonColour, Color.Black, 0.2f));
                else
                    pSpriteBatch.Draw(button.mTexture, button.mRectangle, buttonColour);
            }
        }

    }
}
