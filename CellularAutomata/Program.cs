global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
using ImGuiNET;
using Saffron2D.GuiCollection;
using System.Text;

namespace CellularAutomata
{
    internal class Program
    {
        private static bool paused = false;

        public static void Main()
        {
            DisplayManager.CreateDisplay(1280, 720);

            CellularAutomataSimulation.SimulationSpeed = 20;

            string settingsString;
            bool randomSpawn;

            switch (0)
            {
                // Conway's Game Of Life
                case 1:
                    {
                        settingsString = "2,3/3/1/M";
                        randomSpawn = true;
                    }
                    break;
                // Diamond Fractal
                case 2:
                    {
                        settingsString = "0-8/1/1/N";
                        randomSpawn = false;
                    }
                    break;
                // Diamond
                case 3:
                    {
                        settingsString = "0-8/1-8/1/N";
                        randomSpawn = false;
                    }
                    break;
                // Breathing Diamond
                case 4:
                    {
                        settingsString = "0/1-8/100/N";
                        randomSpawn = false;
                    }
                    break;
                // Cube Fractal
                case 5:
                    {
                        settingsString = "0-8/1/1/M";
                        randomSpawn = false;
                    }
                    break;
                default:
                case 6:
                    {
                        settingsString = "0-4/1-3/3/M";
                        randomSpawn = false;
                    }
                    break;
            }

            int imageWidth = 800;
            int imageHeight = 800;

            if(!Utils.GenerateSettings(settingsString, out CellularAutomataSettings settings))
            {
                return;
            }

            CellularAutomataSimulation cellularAutomataSimulation = new CellularAutomataSimulation(imageWidth, imageHeight, settings, randomSpawn);

            float simulationCounter = 0;

            Sprite sprite = new Sprite(cellularAutomataSimulation.texture);

            sprite.Origin = new Vector2f(cellularAutomataSimulation.texture.Size.X / 2f, cellularAutomataSimulation.texture.Size.Y / 2f);

            sprite.Position = new Vector2f(DisplayManager.ViewWidth / 2f, DisplayManager.ViewHeight / 2f);

            float speed = 200;

            GuiImpl.Init(DisplayManager.Window);

            while (DisplayManager.Window.IsOpen)
            {
                DisplayManager.Window.DispatchEvents();
                DisplayManager.UpdateWindow();
                GuiImpl.Update(DisplayManager.Window, DisplayManager.DeltaTime);

                if ((simulationCounter >= 1f / CellularAutomataSimulation.SimulationSpeed && !paused) || (paused && KeyboardInput.IsKeyPressed(KeyboardKeys.Next)))
                {
                    simulationCounter = 0;
                    sprite.Texture = cellularAutomataSimulation.Simulate();
                }

                if (ImGui.Begin("Stats"))
                {
                    ImGui.Text($"Simulation speed: {CellularAutomataSimulation.SimulationSpeed}");
                    ImGui.Text($"Delta Time: {DisplayManager.DeltaTime}");
                    ImGui.Text($"Current Rule: {settings.rule}");
                    ImGui.Text($"Current Width: {sprite.Texture.Size.X}");
                    ImGui.Text($"Current Height: {sprite.Texture.Size.Y}");
                    ImGui.End();
                }

                if (ImGui.Begin("Options"))
                {
                    ImGui.InputText("Rule", ref settingsString, 512);
                    ImGui.Checkbox("Random Spawn", ref randomSpawn);

                    if(ImGui.InputInt("Width", ref imageWidth))
                    {
                        if(imageWidth <= 0)
                        {
                            imageWidth = 1;
                        }
                    }

                    if(ImGui.InputInt("Height", ref imageHeight))
                    {
                        if (imageHeight <= 0)
                        {
                            imageHeight = 1;
                        }
                    }

                    if (ImGui.Button("Restart"))
                    {
                        if (Utils.GenerateSettings(settingsString, out settings))
                        {
                            cellularAutomataSimulation = new CellularAutomataSimulation(imageWidth, imageHeight, settings, randomSpawn);

                            sprite = new Sprite(cellularAutomataSimulation.texture);

                            sprite.Position = new Vector2f(DisplayManager.ViewWidth / 2f, DisplayManager.ViewHeight / 2f);

                            sprite.Origin = new Vector2f(cellularAutomataSimulation.texture.Size.X / 2f, cellularAutomataSimulation.texture.Size.Y / 2f);
                        }
                    }

                    ImGui.End();
                }

                ProcessInput(speed, cellularAutomataSimulation);

                DisplayManager.Window.Clear(new Color(20, 20, 20));
                DisplayManager.Window.Draw(sprite);
                GuiImpl.Render(DisplayManager.Window);
                DisplayManager.Window.Display();

                if (!paused)
                    simulationCounter += DisplayManager.DeltaTime;

                KeyboardInput.ResetKeyboard();
                MouseInput.ResetMouse();
            }

            GuiImpl.Shutdown();
        }

        private static void ProcessInput(float speed, CellularAutomataSimulation cellularAutomataSimulation)
        {
            View view = DisplayManager.View;
            #region Zoom
            if (MouseInput.Scroll != 0)
            {
                view.Size += new Vector2f(view.Size.X / view.Size.X, view.Size.Y / view.Size.X) * -MouseInput.Scroll * 40;
            }
            #endregion

            #region Speed
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
            #endregion

            #region Movement
            if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveUp))
            {
                view.Move(new Vector2f(0, -1) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1));
            }
            else if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveDown))
            {
                view.Move(new Vector2f(0, 1) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1));
            }

            if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveLeft))
            {
                view.Move(new Vector2f(-1, 0) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1));
            }
            else if (KeyboardInput.IsKeyHeld(KeyboardKeys.MoveRight))
            {
                view.Move(new Vector2f(1, 0) * DisplayManager.DeltaTime * speed * (KeyboardInput.ShiftHeld ? 2 : 1));
            }
            #endregion

            #region Reset
            if (KeyboardInput.IsKeyPressed(KeyboardKeys.Reset))
            {
                cellularAutomataSimulation.ResetCells();
            }
            #endregion

            #region Pause
            if (KeyboardInput.IsKeyPressed(KeyboardKeys.Pause))
            {
                paused = !paused;
            }
            #endregion
            DisplayManager.View = view;
        }
    }
}