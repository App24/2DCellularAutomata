global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;

namespace CellularAutomata
{
    internal class Program
    {
        public static void Main()
        {
            DisplayManager.CreateDisplay(1280, 720);

            CellularAutomataSimulation.SimulationSpeed = 20;

            CellularAutomataSimulation cellularAutomataSimulation = new CellularAutomataSimulation(400, 400, "2,3/3/1/M");

            float simulationCounter = 0;

            Sprite sprite = new Sprite(cellularAutomataSimulation.texture);

            sprite.Origin = new Vector2f(cellularAutomataSimulation.texture.Size.X/2f, cellularAutomataSimulation.texture.Size.Y / 2f);

            sprite.Position = new Vector2f(DisplayManager.ViewWidth / 2f, DisplayManager.ViewHeight / 2f);

            float speed = 200;

            while (DisplayManager.Window.IsOpen)
            {
                DisplayManager.Window.DispatchEvents();
                DisplayManager.UpdateWindow();

                if (simulationCounter >= 1f / CellularAutomataSimulation.SimulationSpeed)
                {
                    simulationCounter = 0;
                    sprite.Texture = cellularAutomataSimulation.Simulate();
                }

                if (KeyboardInput.IsKeyHeld(KeyboardKeys.ZoomIn))
                {
                    sprite.Scale += new Vector2f(1, 1) * DisplayManager.DeltaTime;
                }
                else if (KeyboardInput.IsKeyHeld(KeyboardKeys.ZoomOut))
                {
                    sprite.Scale -= new Vector2f(1, 1) * DisplayManager.DeltaTime;
                }

                if (KeyboardInput.IsKeyPressed(KeyboardKeys.SpeedUp))
                {
                    CellularAutomataSimulation.SimulationSpeed += 5;
                }
                else if (KeyboardInput.IsKeyPressed(KeyboardKeys.SpeedDown))
                {
                    CellularAutomataSimulation.SimulationSpeed -= 5;
                    if (CellularAutomataSimulation.SimulationSpeed <= 0)
                    {
                        CellularAutomataSimulation.SimulationSpeed = 5;
                    }
                }

                if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveUp))
                {
                    sprite.Position+=new Vector2f(0, 1) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1);
                }
                else if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveDown))
                {
                    sprite.Position -= new Vector2f(0, 1) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1);
                }

                if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveLeft))
                {
                    sprite.Position += new Vector2f(1, 0) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1);
                }
                else if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveRight))
                {
                    sprite.Position -= new Vector2f(1, 0) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1);
                }

                DisplayManager.Window.Clear(new Color(20,20,20));
                DisplayManager.Window.Draw(sprite);
                DisplayManager.Window.Display();

                simulationCounter += DisplayManager.DeltaTime;

                KeyboardInput.ResetKeyboard();
            }
        }
    }
}