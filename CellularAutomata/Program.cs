global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
using ImGuiNET;
using Saffron2D.GuiCollection;
using System.Text;

namespace CellularAutomata
{
    internal struct Preset
    {
        public string name;
        public string rule;
        public bool randomSpawn;

        public Preset(string name, string rule, bool randomSpawn)
        {
            this.name = name;
            this.rule = rule;
            this.randomSpawn = randomSpawn;
        }
    }

    internal class Program
    {
        private static List<Preset> presets = new List<Preset>()
            {
                new Preset("GOF", "2,3/3/1/M", true),
                new Preset("Diamond Fractal", "0-8/1/1/N", false),
                new Preset("Diamond", "0-8/1-8/1/N", false),
                new Preset("Breathing Diamond", "0/1-8/10/N", false),
                new Preset("Square Fractal", "0-8/1/1/M", false),
                new Preset("Flashing Squares", "0/1/1/M", false),
                new Preset("Twinkling Stars", "3,4/3/1/M", true),
            };

        private static List<string> presetNames
        {
            get
            {
                List<string> list = new List<string>();

                foreach (Preset item in presets)
                {
                    list.Add(item.name);
                }

                return list;
            }
        }

        private static int presetIndex = 0;

        private static Preset CurrentPreset => presets[presetIndex];

        public static void Main()
        {
            DisplayManager.CreateDisplay(1280, 720);

            CellularAutomataSimulation.simulationSpeed = 20;

            string settingsString = CurrentPreset.rule;
            bool randomSpawn = CurrentPreset.randomSpawn;

            int imageWidth = 800;
            int imageHeight = 800;

            if (!Utils.GenerateSettings(settingsString, out CellularAutomataSettings settings))
            {
                return;
            }

            System.Numerics.Vector3 cellColor = new System.Numerics.Vector3(1);
            System.Numerics.Vector3 backgroundColor = new System.Numerics.Vector3(0);

            CellularAutomataSimulation cellularAutomataSimulation = new CellularAutomataSimulation(imageWidth, imageHeight, settings, randomSpawn);

            cellularAutomataSimulation.cellColor = new Vector3f(cellColor.X, cellColor.Y, cellColor.Z);
            cellularAutomataSimulation.backgroundColor = new Vector3f(backgroundColor.X, backgroundColor.Y, backgroundColor.Z); ;

            float simulationCounter = 0;

            Sprite sprite = new Sprite(cellularAutomataSimulation.texture);

            sprite.Origin = new Vector2f(cellularAutomataSimulation.texture.Size.X / 2f, cellularAutomataSimulation.texture.Size.Y / 2f);

            sprite.Position = new Vector2f(DisplayManager.WindowWidth / 2f, DisplayManager.WindowHeight / 2f);

            float speed = 200;

            bool paused = false;

            bool unlockedSpeed = false;

            GuiImpl.Init(DisplayManager.Window);

            while (DisplayManager.Window.IsOpen)
            {
                DisplayManager.Window.DispatchEvents();
                DisplayManager.UpdateWindow();
                GuiImpl.Update(DisplayManager.Window, DisplayManager.DeltaTime);

                if ((simulationCounter >= 1f / CellularAutomataSimulation.simulationSpeed) || (unlockedSpeed && !paused))
                {
                    simulationCounter = 0;
                    sprite.Texture = cellularAutomataSimulation.Simulate();
                }

                if (ImGui.Begin("Simulation Stats"))
                {
                    ImGui.Text($"Simulation speed: {(unlockedSpeed ? "Unlocked" : CellularAutomataSimulation.simulationSpeed)}");
                    ImGui.Text($"Delta Time: {DisplayManager.DeltaTime}");
                    ImGui.Text($"Current Rule: {settings.rule}");
                    ImGui.Text($"Current Width: {sprite.Texture.Size.X}");
                    ImGui.Text($"Current Height: {sprite.Texture.Size.Y}");
                    ImGui.Text($"Number of Cells: {sprite.Texture.Size.X * sprite.Texture.Size.Y}");
                    ImGui.End();
                }

                if (ImGui.Begin("Color"))
                {
                    if (ImGuiExtras.ColorPicker("Cell Color", ref cellColor))
                    {
                        cellularAutomataSimulation.cellColor = new Vector3f(cellColor.X, cellColor.Y, cellColor.Z);
                    }

                    if (ImGuiExtras.ColorPicker("Background Color", ref backgroundColor))
                    {
                        cellularAutomataSimulation.backgroundColor = new Vector3f(backgroundColor.X, backgroundColor.Y, backgroundColor.Z);
                    }

                    ImGui.End();
                }

                if (ImGui.Begin("Simulation Settings"))
                {
                    ImGui.Checkbox("Unlocked Speed", ref unlockedSpeed);

                    if (!unlockedSpeed)
                    {
                        if (ImGui.InputInt("Speed", ref CellularAutomataSimulation.simulationSpeed))
                        {
                            if (CellularAutomataSimulation.simulationSpeed <= 0)
                            {
                                CellularAutomataSimulation.simulationSpeed = 1;
                            }
                        }
                    }

                    if (ImGui.Button(paused ? "Unpause" : "Pause"))
                    {
                        paused = !paused;
                    }

                    if (paused)
                    {
                        if (ImGui.Button("Next Frame"))
                        {
                            simulationCounter = 1;
                        }
                    }

                    ImGui.End();
                }

                if (ImGui.Begin("Automata Settings"))
                {
                    if(ImGuiExtras.Dropdown("##combo", presetNames.ToArray(), ref presetIndex))
                    {
                        settingsString = CurrentPreset.rule;
                        randomSpawn = CurrentPreset.randomSpawn;
                    }

                    ImGui.InputText("Rule", ref settingsString, 512);
                    ImGui.Checkbox("Random Spawn", ref randomSpawn);

                    if (ImGui.InputInt("Width", ref imageWidth))
                    {
                        if (imageWidth <= 0)
                        {
                            imageWidth = 1;
                        }
                    }

                    if (ImGui.InputInt("Height", ref imageHeight))
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

                            cellularAutomataSimulation.cellColor = new Vector3f(cellColor.X, cellColor.Y, cellColor.Z);
                            cellularAutomataSimulation.backgroundColor = new Vector3f(backgroundColor.X, backgroundColor.Y, backgroundColor.Z);

                            sprite = new Sprite(cellularAutomataSimulation.texture);

                            sprite.Position = new Vector2f(DisplayManager.WindowWidth / 2f, DisplayManager.WindowHeight / 2f);

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
            if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow))
            {
                if (MouseInput.Scroll != 0)
                {
                    view.Size += new Vector2f(view.Size.X / view.Size.X, view.Size.Y / view.Size.X) * -MouseInput.Scroll * 40;
                }
            }
            #endregion

            #region Movement
            if (!ImGui.IsAnyItemActive())
            {
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
            }
            #endregion
            DisplayManager.View = view;
        }
    }
}