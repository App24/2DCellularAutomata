using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellularAutomata
{
    internal static class DisplayManager
    {
        public static RenderWindow Window { get; private set; }

        public static uint WindowWidth { get { return Window.Size.X; } }
        public static uint WindowHeight { get { return Window.Size.Y; } }

        public static View View { get { return Window.GetView(); } set { Window.SetView(value); } }

        public static float ViewWidth { get { return View.Size.X; } }
        public static float ViewHeight { get { return View.Size.Y; } }

        public static float DeltaTime { get; private set; }

        private static long previousTime;

        public static void CreateDisplay(uint width, uint height)
        {
            Window = new RenderWindow(new VideoMode(width, height), "Cellular Automata", Styles.Close | Styles.Titlebar);

            View = new View(new Vector2f(width / 2f, height / 2f), new Vector2f(width, height));

            Window.Closed += (_, _) => Close();

            KeyboardInput.InitKeyboard();

            previousTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static void Close()
        {
            Window.Close();
        }

        public static void UpdateWindow()
        {
            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            DeltaTime = (currentTime - previousTime) / 1000f;
            previousTime = currentTime;
        }
    }
}
