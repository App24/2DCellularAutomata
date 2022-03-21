using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellularAutomata
{
    internal static class MouseInput
    {
        private static bool[] buttonsHeld = new bool[(int)Mouse.Button.ButtonCount];
        private static bool[] buttonsPressed = new bool[(int)Mouse.Button.ButtonCount];
        private static bool[] buttonsPressedThisFrame = new bool[(int)Mouse.Button.ButtonCount];

        public static float Scroll { get; private set; }

        public static Vector2f NormalisedMousePos
        {
            get { return new Vector2f(MouseX / (float)DisplayManager.WindowWidth, MouseY / (float)DisplayManager.WindowHeight); }
        }

        public static Vector2i MousePos { get; private set; }
        public static int MouseX { get { return MousePos.X; } }
        public static int MouseY { get { return MousePos.Y; } }

        public static void InitMouse()
        {
            DisplayManager.Window.MouseButtonPressed += ButtonPressed;
            DisplayManager.Window.MouseButtonReleased += ButtonReleased;
            DisplayManager.Window.MouseWheelScrolled += MouseWheelScrolled;
            DisplayManager.Window.MouseMoved += MouseMoved;
        }

        public static bool IsButtonHeld(Mouse.Button button)
        {
            return buttonsHeld[(int)button];
        }

        public static bool IsButtonPressed(Mouse.Button button)
        {
            if (buttonsPressedThisFrame[(int)button])
                return true;
            if (buttonsHeld[(int)button] && !buttonsPressed[(int)button])
            {
                buttonsPressed[(int)button] = true;
                buttonsPressedThisFrame[(int)button] = true;
                return true;
            }
            else if (!buttonsHeld[(int)button])
            {
                buttonsPressed[(int)button] = false;
            }
            return false;
        }

        public static void ResetMouse()
        {
            for (int i = 0; i < buttonsPressedThisFrame.Length; i++)
            {
                buttonsPressedThisFrame[i] = false;
            }
            Scroll = 0;
        }

        #region Events
        private static void ButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (e.Button < 0 || e.Button > Mouse.Button.ButtonCount)
                return;
            buttonsHeld[(int)e.Button] = true;
        }

        private static void ButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.Button < 0 || e.Button > Mouse.Button.ButtonCount)
                return;
            buttonsHeld[(int)e.Button] = false;
        }

        private static void MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            Scroll += e.Delta;
        }

        private static void MouseMoved(object sender, MouseMoveEventArgs e)
        {
            MousePos = new Vector2i(e.X, e.Y);
        }
        #endregion
    }
}
