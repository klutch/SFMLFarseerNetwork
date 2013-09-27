using System;
using SFML.Window;

namespace SFML_Farseer_Network
{
    public struct KeyboardState
    {
        private bool[] _keysPressed;

        public KeyboardState(bool[] keysPressed)
        {
            _keysPressed = keysPressed;
        }

        public bool isPressed(Keyboard.Key key)
        {
            int iKey = (int)key;

            if (iKey < 0 || iKey > 100)
            {
                return false;
            }

            return _keysPressed[iKey];
        }

        public bool isReleased(Keyboard.Key key)
        {
            int iKey = (int)key;

            if (iKey < 0 || iKey > 100)
            {
                return true;
            }

            return !_keysPressed[iKey];
        }

        public static KeyboardState get()
        {
            bool[] keysPressed = new bool[101];
            int[] values = Enum.GetValues(typeof(Keyboard.Key)) as int[];

            for (int i = -1; i < 100; i++)
            {
                int j = i + 1;
                keysPressed[j] = Keyboard.IsKeyPressed((Keyboard.Key)j);
            }

            return new KeyboardState(keysPressed);
        }
    }
}
