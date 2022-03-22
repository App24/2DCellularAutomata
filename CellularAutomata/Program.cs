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
                new Preset("Squre Fractal", "0-8/1/1/M", false),
                new Preset("Flashing Squares", "0/1/1/M", false),
                new Preset("Twinkling Stars", "3,4/3/1/M", true),
            };
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

            System.Numerics.Vector3 rgb = new System.Numerics.Vector3(1);
            CellularAutomataSimulation cellularAutomataSimulation = new CellularAutomataSimulation(imageWidth, imageHeight, settings, randomSpawn);

            cellularAutomataSimulation.cellColor = new Vector3f(rgb.X, rgb.Y, rgb.Z);

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

                if (ImGui.Begin("Color"))
                {
                    if(ColorPicker("Cell Color", ref rgb))
                    {
                        cellularAutomataSimulation.cellColor = new Vector3f(rgb.X, rgb.Y, rgb.Z);
                    }
                    ImGui.End();
                }

                if (ImGui.Begin("Stats"))
                {
                    ImGui.Text($"Simulation speed: {(unlockedSpeed ? "Unlocked" : CellularAutomataSimulation.simulationSpeed)}");
                    ImGui.Text($"Delta Time: {DisplayManager.DeltaTime}");
                    ImGui.Text($"Current Rule: {settings.rule}");
                    ImGui.Text($"Current Width: {sprite.Texture.Size.X}");
                    ImGui.Text($"Current Height: {sprite.Texture.Size.Y}");
                    ImGui.Text($"Number of Cells: {sprite.Texture.Size.X * sprite.Texture.Size.Y}");
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
                    if (ImGui.BeginCombo("##combo", CurrentPreset.name))
                    {
                        for (int i = 0; i < presets.Count; i++)
                        {
                            string name = presets[i].name;
                            bool selected = name == CurrentPreset.name;
                            if (ImGui.Selectable(name, selected))
                            {
                                presetIndex = i;
                                settingsString = CurrentPreset.rule;
                                randomSpawn = CurrentPreset.randomSpawn;
                            }
                            if (selected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                        ImGui.EndCombo();
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

                            cellularAutomataSimulation.cellColor = new Vector3f(rgb.X, rgb.Y, rgb.Z);

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
            if (MouseInput.Scroll != 0)
            {
                view.Size += new Vector2f(view.Size.X / view.Size.X, view.Size.Y / view.Size.X) * -MouseInput.Scroll * 40;
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
            DisplayManager.View = view;
        }

        static bool ColorPicker(string label, ref System.Numerics.Vector3 rgb)
        {
            float HUE_PICKER_WIDTH = 20f;
            float CROSSHAIR_SIZE = 7f;
            System.Numerics.Vector2 SV_PICKER_SIZE = new System.Numerics.Vector2(200, 200);

            ImColor color = new ImColor();
            color.Value = new System.Numerics.Vector4(rgb, 255);
            bool valueChanged = false;

            var drawList = ImGui.GetWindowDrawList();

            var pickerPos = ImGui.GetCursorScreenPos();

            List<ImColor> colors = new List<ImColor>();

            ImColor customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(255, 0, 0, 255);
            colors.Add(customColor);

            customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(255, 255, 0, 255);
            colors.Add(customColor);

            customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(0, 255, 0, 255);
            colors.Add(customColor);

            customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(0, 255, 255, 255);
            colors.Add(customColor);

            customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(0, 0, 255, 255);
            colors.Add(customColor);

            customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(255, 0, 255, 255);
            colors.Add(customColor);

            customColor = new ImColor();
            customColor.Value = new System.Numerics.Vector4(255, 0, 0, 255);
            colors.Add(customColor);

            for (int i = 0; i < 6; i++)
            {
                drawList.AddRectFilledMultiColor(
                    new System.Numerics.Vector2(pickerPos.X + SV_PICKER_SIZE.X + 10, pickerPos.Y + i * (SV_PICKER_SIZE.Y / 6f)),
                    new System.Numerics.Vector2(pickerPos.X + SV_PICKER_SIZE.X + 10 + HUE_PICKER_WIDTH, pickerPos.Y + (i + 1) * (SV_PICKER_SIZE.Y / 6)),
                    ImGui.ColorConvertFloat4ToU32(colors[i].Value),
                    ImGui.ColorConvertFloat4ToU32(colors[i].Value),
                    ImGui.ColorConvertFloat4ToU32(colors[i + 1].Value),
                    ImGui.ColorConvertFloat4ToU32(colors[i + 1].Value)
                    );
            }

            ImGui.ColorConvertRGBtoHSV(
                color.Value.X, color.Value.Y, color.Value.Z, out float hue, out float saturation, out float value);

            drawList.AddLine(
                new System.Numerics.Vector2(pickerPos.X + SV_PICKER_SIZE.X + 8, pickerPos.Y + hue * SV_PICKER_SIZE.Y),
                new System.Numerics.Vector2(pickerPos.X + SV_PICKER_SIZE.X + 12 + HUE_PICKER_WIDTH, pickerPos.Y + hue * SV_PICKER_SIZE.Y),
                ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(255))
                );

            {
                int step = 5;
                System.Numerics.Vector2 pos = new System.Numerics.Vector2(0);

                System.Numerics.Vector4 c00 = new System.Numerics.Vector4(1, 1, 1, 1);
                System.Numerics.Vector4 c10 = new System.Numerics.Vector4(1, 1, 1, 1);
                System.Numerics.Vector4 c01 = new System.Numerics.Vector4(1, 1, 1, 1);
                System.Numerics.Vector4 c11 = new System.Numerics.Vector4(1, 1, 1, 1);
                for (int y1 = 0; y1 < step; y1++)
                {
                    for (int x1 = 0; x1 < step; x1++)
                    {
                        float s0 = x1 / (float)step;
                        float s1 = (x1 + 1) / (float)step;
                        float v0 = 1 - y1 / (float)step;
                        float v1 = 1 - (y1+1) / (float)step;

                        ImGui.ColorConvertHSVtoRGB(hue, s0, v0, out c00.X, out c00.Y, out c00.Z);
                        ImGui.ColorConvertHSVtoRGB(hue, s1, v0, out c10.X, out c10.Y, out c10.Z);
                        ImGui.ColorConvertHSVtoRGB(hue, s0, v1, out c01.X, out c01.Y, out c01.Z);
                        ImGui.ColorConvertHSVtoRGB(hue, s1, v1, out c11.X, out c11.Y, out c11.Z);

                        drawList.AddRectFilledMultiColor(
                            new System.Numerics.Vector2(pickerPos.X + pos.X, pickerPos.Y+pos.Y),
                            new System.Numerics.Vector2(pickerPos.X + pos.X + SV_PICKER_SIZE.X/step, pickerPos.Y+pos.Y+SV_PICKER_SIZE.Y/step),
                            ImGui.ColorConvertFloat4ToU32(c00),
                            ImGui.ColorConvertFloat4ToU32(c10),
                            ImGui.ColorConvertFloat4ToU32(c11),
                            ImGui.ColorConvertFloat4ToU32(c01)
                            );

                        pos.X += SV_PICKER_SIZE.X / step;
                    }
                    pos.X = 0;
                    pos.Y += SV_PICKER_SIZE.Y / step;
                }
            }

            float x = saturation * SV_PICKER_SIZE.X;
            float y = (1-value) * SV_PICKER_SIZE.Y;
            System.Numerics.Vector2 p = new System.Numerics.Vector2(pickerPos.X + x, pickerPos.Y + y);
            drawList.AddLine(new System.Numerics.Vector2(p.X - CROSSHAIR_SIZE, p.Y), new System.Numerics.Vector2(p.X - 2, p.Y), ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(255)));
            drawList.AddLine(new System.Numerics.Vector2(p.X + CROSSHAIR_SIZE, p.Y), new System.Numerics.Vector2(p.X + 2, p.Y), ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(255)));
            drawList.AddLine(new System.Numerics.Vector2(p.X, p.Y - CROSSHAIR_SIZE), new System.Numerics.Vector2(p.X, p.Y - 2), ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(255)));
            drawList.AddLine(new System.Numerics.Vector2(p.X, p.Y + CROSSHAIR_SIZE), new System.Numerics.Vector2(p.X, p.Y + 2), ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(255)));

            ImGui.InvisibleButton("saturation_value_selector", SV_PICKER_SIZE);

            if (ImGui.IsItemActive() && ImGui.GetIO().MouseDown[0])
            {
                System.Numerics.Vector2 mousePosInCanvas = new System.Numerics.Vector2(ImGui.GetIO().MousePos.X - pickerPos.X, ImGui.GetIO().MousePos.Y - pickerPos.Y);

                if (mousePosInCanvas.X < 0) mousePosInCanvas.X = 0;
                else if (mousePosInCanvas.X >= SV_PICKER_SIZE.X - 1) mousePosInCanvas.X = SV_PICKER_SIZE.X - 1;

                if (mousePosInCanvas.Y < 0) mousePosInCanvas.Y = 0;
                else if (mousePosInCanvas.Y >= SV_PICKER_SIZE.Y - 1) mousePosInCanvas.Y = SV_PICKER_SIZE.Y - 1;

                value = 1 - (mousePosInCanvas.Y / (SV_PICKER_SIZE.Y - 1));
                saturation = mousePosInCanvas.X / (SV_PICKER_SIZE.X - 1);

                valueChanged = true;
            }

            ImGui.SetCursorScreenPos(new System.Numerics.Vector2(pickerPos.X + SV_PICKER_SIZE.X + 10, pickerPos.Y));
            ImGui.InvisibleButton("hue_selector", new System.Numerics.Vector2(HUE_PICKER_WIDTH, SV_PICKER_SIZE.Y));

            if((ImGui.IsItemHovered()||ImGui.IsItemActive()) && ImGui.GetIO().MouseDown[0])
            {
                System.Numerics.Vector2 mousePosInCanvas = new System.Numerics.Vector2(ImGui.GetIO().MousePos.X - pickerPos.X, ImGui.GetIO().MousePos.Y - pickerPos.Y);

                if (mousePosInCanvas.Y < 0) mousePosInCanvas.Y = 0;
                else if (mousePosInCanvas.Y >= SV_PICKER_SIZE.Y - 2) mousePosInCanvas.Y = SV_PICKER_SIZE.Y - 2;

                hue = mousePosInCanvas.Y / (SV_PICKER_SIZE.Y - 1);

                valueChanged = true;
            }

            ImGui.ColorConvertHSVtoRGB(hue, saturation, value, out rgb.X, out rgb.Y, out rgb.Z);

            return valueChanged | ImGui.ColorEdit3(label, ref rgb);
        }
    }
}