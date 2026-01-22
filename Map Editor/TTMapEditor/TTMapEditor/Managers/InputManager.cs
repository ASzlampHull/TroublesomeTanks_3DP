using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TTMapEditor.Managers
{
    internal class InputManager
    {
        static InputManager mInstance = new InputManager();
        private static KeyboardState mCurrentState;
        private static KeyboardState mPreviousState;
        private static MouseState mCurrentMouseState;
        private static MouseState mPreviousMouseState;


        public static InputManager Instance
        {
            get { return mInstance; }
        }

        public static void Update()
        {
            mPreviousState = mCurrentState;
            mCurrentState = Keyboard.GetState();
            mPreviousMouseState = mCurrentMouseState;
            mCurrentMouseState = Mouse.GetState();
        }

        public static bool isKeyPressed(Keys key)
        {
            return mCurrentState.IsKeyDown(key) && mPreviousState.IsKeyUp(key);
        }

        public static bool isKeyReleased(Keys key)
        {
            return mCurrentState.IsKeyUp(key) && mPreviousState.IsKeyDown(key);
        }

        public static bool wasKeyPressed(Keys key)
        {
            return mPreviousState.IsKeyDown(key);
        }

        public static bool wasKeyReleased(Keys key)
        {
            return mPreviousState.IsKeyUp(key);
        }

        // Return the MonoGame Vector2 so callers (scenes/objects) use the same type
        public static Vector2 GetMousePosition()
        {
            return new Vector2(mCurrentMouseState.X, mCurrentMouseState.Y);
        }

        public static bool isLeftMouseClicked()
        {
            return mCurrentMouseState.LeftButton == ButtonState.Pressed && mPreviousMouseState.LeftButton == ButtonState.Released;
        }

        public static bool isLeftMouseReleased()
        {
            return mCurrentMouseState.LeftButton == ButtonState.Released && mPreviousMouseState.LeftButton == ButtonState.Pressed;
        }
    }
}
