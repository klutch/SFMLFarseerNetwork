using System;
using SFML.Window;

namespace SFML_Farseer_Network
{
    public struct MouseState
    {
        private bool _leftButtonPressed;
        private bool _rightButtonPressed;

        public bool isLeftButtonPressed { get { return _leftButtonPressed; } }
        public bool isRightButtonPressed { get { return _rightButtonPressed; } }

        public MouseState(bool leftButtonPressed, bool rightButtonPressed)
        {
            _leftButtonPressed = leftButtonPressed;
            _rightButtonPressed = rightButtonPressed;
        }

        public static MouseState get()
        {
            return new MouseState(Mouse.IsButtonPressed(Mouse.Button.Left), Mouse.IsButtonPressed(Mouse.Button.Right));
        }
    }
}
