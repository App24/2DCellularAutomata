namespace CellularAutomata
{
    internal struct KeyboardKeys
    {
        public static Keyboard.Key[] ZoomIn = { Keyboard.Key.Add };
        public static Keyboard.Key[] ZoomOut = { Keyboard.Key.Subtract };
        public static Keyboard.Key[] SpeedUp = { Keyboard.Key.Up };
        public static Keyboard.Key[] SpeedDown = { Keyboard.Key.Down };
        public static Keyboard.Key[] MoveUp = { Keyboard.Key.W };
        public static Keyboard.Key[] MoveLeft = { Keyboard.Key.A };
        public static Keyboard.Key[] MoveRight = { Keyboard.Key.D };
        public static Keyboard.Key[] MoveDown = { Keyboard.Key.S };
    }

    internal static class KeyboardInput
    {
        private static bool[] keysHeld = new bool[(int)Keyboard.Key.KeyCount];
        private static bool[] keysPressed = new bool[(int)Keyboard.Key.KeyCount];
        private static bool[] keysPressedThisFrame = new bool[(int)Keyboard.Key.KeyCount];

        public static bool AltHeld { get; private set; }
        public static bool ShiftHeld { get; private set; }
        public static bool CtrlHeld { get; private set; }

        public static void InitKeyboard()
        {
            DisplayManager.Window.KeyReleased += KeyReleased;
            DisplayManager.Window.KeyPressed += KeyPressed;
        }

        public static bool IsKeyPressed(params Keyboard.Key[] keys)
        {
            foreach (Keyboard.Key key in keys)
            {
                if (keysPressedThisFrame[(int)key])
                    return true;
                if (keysHeld[(int)key] && !keysPressed[(int)key])
                {
                    keysPressed[(int)key] = true;
                    keysPressedThisFrame[(int)key] = true;
                    return true;
                }
                else if (!keysHeld[(int)key])
                {
                    keysPressed[(int)key] = false;
                }
            }
            return false;
        }

        public static bool IsKeyHeld(params Keyboard.Key[] keys)
        {
            foreach (Keyboard.Key key in keys)
            {
                if (keysHeld[(int)key])
                    return true;
            }
            return false;
        }

        public static void ResetKeyboard()
        {
            for (int i = 0; i < keysPressedThisFrame.Length; i++)
            {
                keysPressedThisFrame[i] = false;
            }
        }

        #region Events
        private static void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code < 0 || e.Code > Keyboard.Key.KeyCount)
                return;
            keysHeld[(int)e.Code] = true;
            AltHeld = e.Alt;
            CtrlHeld = e.Control;
            ShiftHeld = e.Shift;
        }

        private static void KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code < 0 || e.Code > Keyboard.Key.KeyCount)
                return;
            keysHeld[(int)e.Code] = false;
            AltHeld = e.Alt;
            CtrlHeld = e.Control;
            ShiftHeld = e.Shift;
        }
        #endregion
    }
}
