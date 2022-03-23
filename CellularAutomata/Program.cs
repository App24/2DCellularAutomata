global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
using ImGuiNET;
using ImGuiNETSFML;
using System.Text;

namespace CellularAutomata
{
    internal struct Preset
    {
        public string name;
        public string rule;
        public CellPreset spawn;

        public Preset(string name, string rule, CellPreset spawn)
        {
            this.name = name;
            this.rule = rule;
            this.spawn = spawn;
        }
    }

    enum CellPreset
    {
        Random,
        Middle,
        MiddleCircle,
        HorizontalLine,
        VerticalLine,
        Plus
    }

    internal class Program
    {
        private static List<Preset> presets = new List<Preset>()
            {
                new Preset("GOF", "2,3/3/1/M", CellPreset.Random),
                new Preset("Diamond Fractal", "0-8/1/1/N", CellPreset.Middle),
                new Preset("Diamond", "0-8/1-8/1/N", CellPreset.Middle),
                new Preset("Breathing Diamond", "0/1-8/10/N", CellPreset.Middle),
                new Preset("Square Fractal", "0-8/1/1/M", CellPreset.Middle),
                new Preset("Flashing Squares", "0/1/1/M", CellPreset.Middle),
                new Preset("Twinkling Stars", "3,4/3/1/M", CellPreset.Random),
                new Preset("Growth", "2-6/2/1/M", CellPreset.MiddleCircle),
                new Preset("Growing GOF", "2,3/3/1/M", CellPreset.Plus),
            };

        private static List<string> PresetNames
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

        static List<string> cellPresets = new List<string>(Enum.GetNames(typeof(CellPreset)));

        private static int presetIndex = 0;
        static int cellPreset=0;

        private static Preset CurrentPreset => presets[presetIndex];
        private static CellPreset CurrentCellPreset => (CellPreset)cellPreset;

        public static void Main()
        {
            DisplayManager.CreateDisplay(1280, 720);

            CellularAutomataSimulation.simulationSpeed = 20;

            string settingsString = CurrentPreset.rule;

            int imageWidth = 800;
            int imageHeight = 800;

            if (!Utils.GenerateSettings(settingsString, out CellularAutomataSettings settings))
            {
                return;
            }

            Vector3f cellColor = new Vector3f(1, 1, 1);
            Vector3f backgroundColor = new Vector3f(0, 0, 0);

            CellularAutomataSimulation cellularAutomataSimulation = new (imageWidth, imageHeight, settings, CurrentCellPreset, cellColor, backgroundColor);

            float simulationCounter = 0;

            Sprite sprite = new Sprite(cellularAutomataSimulation.texture);

            sprite.Origin = new Vector2f(cellularAutomataSimulation.texture.Size.X / 2f, cellularAutomataSimulation.texture.Size.Y / 2f);

            sprite.Position = new Vector2f(DisplayManager.WindowWidth / 2f, DisplayManager.WindowHeight / 2f);

            float speed = 200;

            bool paused = false;

            bool unlockedSpeed = false;

            float clock = 0;

            int currentGeneration = 0;
            int generationDifference = 0;

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
                    currentGeneration++;
                }

                if(clock >= 1)
                {
                    clock = 0;
                    generationDifference = currentGeneration;
                    currentGeneration = 0;
                }

                if (ImGui.Begin("Simulation Stats"))
                {
                    ImGui.Text($"Simulation speed: {(unlockedSpeed ? "Unlocked" : CellularAutomataSimulation.simulationSpeed)}");
                    ImGui.Text($"Generations Per Second: {generationDifference}");
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
                        cellularAutomataSimulation.cellColor = cellColor;
                    }

                    if (ImGuiExtras.ColorPicker("Background Color", ref backgroundColor))
                    {
                        cellularAutomataSimulation.backgroundColor = backgroundColor;
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

                    if(ImGui.InputInt("Threads", ref CellularAutomataSimulation.threadCount))
                    {
                        if(CellularAutomataSimulation.threadCount <= 0)
                        {
                            CellularAutomataSimulation.threadCount = 1;
                        }

                        cellularAutomataSimulation.CreateThreads();
                    }

                    ImGui.End();
                }

                if (ImGui.Begin("Automata Settings"))
                {
                    if(ImGuiExtras.Dropdown("Presets", PresetNames.ToArray(), ref presetIndex))
                    {
                        settingsString = CurrentPreset.rule;
                        cellPreset = (int)CurrentPreset.spawn;
                    }

                    ImGui.InputText("Rule", ref settingsString, 512);

                    ImGuiExtras.Dropdown("Placement", cellPresets.ToArray(), ref cellPreset);

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
                            cellularAutomataSimulation = new CellularAutomataSimulation(imageWidth, imageHeight, settings, CurrentCellPreset, cellColor, backgroundColor);

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

                clock += DisplayManager.DeltaTime;

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
                    view.Size += new Vector2f(view.Size.X / view.Size.X, view.Size.Y / view.Size.X) * -MouseInput.Scroll * 40 * (KeyboardInput.ShiftHeld ? 2 : 1);
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