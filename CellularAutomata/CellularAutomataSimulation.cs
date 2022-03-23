﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellularAutomata
{
    internal class CellularAutomataSimulation
    {
        public static int simulationSpeed;

        private readonly int width;
        private readonly int height;
        private readonly CellularAutomataSettings settings;
        private Action[] threadActions;
        public static int threadCount = 8;
        private readonly List<int> remainingLines = new List<int>();

        private byte[] cells;
        private byte[] updateCells;

        public readonly Texture texture;

        private readonly CellPreset spawn;

        public Vector3f cellColor;
        public Vector3f backgroundColor;

        public CellularAutomataSimulation(int width, int height, CellularAutomataSettings settings, CellPreset spawn, Vector3f cellColor, Vector3f backgroundColor)
        {
            this.width = width;
            this.height = height;
            this.settings = settings;
            this.spawn = spawn;
            this.cellColor = cellColor;
            this.backgroundColor = backgroundColor;
            ResetCells();
            texture = new Texture((uint)width, (uint)height);

            CreateThreads();

            UpdateTexture();
        }

        public void CreateThreads()
        {
            threadActions = new Action[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threadActions[i] = () => SimulateThread();
            }
        }

        public void ResetCells()
        {
            cells = new byte[width * height];
            switch (spawn)
            {
                default:
                case CellPreset.Random:
                    {
                        Random random = new Random();
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                bool spawn = random.Next(0, 2) >= 1;
                                if (spawn)
                                {
                                    cells[GetIndex(x, y)] = settings.states;
                                }
                            }
                        }
                    }
                    break;
                case CellPreset.Middle:
                    {
                        cells[GetIndex((int)(width / 2f), (int)(height / 2f))] = settings.states;
                    }
                    break;
                case CellPreset.MiddleCircle:
                    {
                        int midX = (int)(width / 2f);
                        int midY = (int)(height / 2f);
                        int radius = 10;

                        for (int x = -radius; x < radius; x++)
                        {
                            for (int y = -radius; y < radius; y++)
                            {
                                if((x * x + y * y) <= radius*radius)
                                {
                                    cells[GetIndex(midX + x, midY + y)] = settings.states;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void SimulateThread()
        {
            while (remainingLines.Count > 0)
            {
                int line = 0;
                lock (remainingLines)
                {
                    if (remainingLines.Count <= 0)
                        break;
                    line = remainingLines[0];
                    remainingLines.RemoveAt(0);
                }

                for (int i = 0; i < width; i++)
                {
                    updateCells[GetIndex(i, line)] = SimulateCell(i, line);
                }
            }
        }

        public Texture Simulate()
        {
            for (int i = 0; i < height; i++)
            {
                remainingLines.Add(i);
            }

            updateCells = new byte[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                updateCells[i] = cells[i];
            }

            Parallel.Invoke(threadActions);

            cells = updateCells;

            UpdateTexture();
            return texture;
        }

        private byte SimulateCell(int x, int y)
        {
            byte[] neighbourCells = new byte[settings.neighbourhoodMode == NeighbourhoodMode.VonNeuman ? 4 : 8];
            neighbourCells[0] = GetCell(x, y - 1);
            neighbourCells[1] = GetCell(x, y + 1);
            neighbourCells[2] = GetCell(x - 1, y);
            neighbourCells[3] = GetCell(x + 1, y);

            if (settings.neighbourhoodMode == NeighbourhoodMode.Moore)
            {
                neighbourCells[4] = GetCell(x - 1, y - 1);
                neighbourCells[5] = GetCell(x + 1, y - 1);
                neighbourCells[6] = GetCell(x - 1, y + 1);
                neighbourCells[7] = GetCell(x + 1, y + 1);
            }

            byte count = 0;

            for (int i = 0; i < neighbourCells.Length; i++)
            {
                if (neighbourCells[i] == settings.states)
                {
                    count++;
                }
            }

            byte cell = GetCell(x, y);
            if (cell == settings.states)
            {
                if (settings.survive.Contains(count))
                {
                    return settings.states;
                }
                return (byte)(settings.states - 1);
            }
            else if (cell > 0 && cell < settings.states)
            {
                return (byte)(cell - 1);
            }
            else
            {
                if (settings.born.Contains(count))
                    return settings.states;
                return 0;
            }
        }

        private void UpdateTexture()
        {
            byte[] data = new byte[width * height * 4];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = y * width + x;
                    int dataIndex = (y * width + x) * 4;
                    data[dataIndex + 0] = (byte)(Math.Round(cellColor.X * (255 - (backgroundColor.X * 255)) * (cells[index] / (float)settings.states)) + (backgroundColor.X * 255));
                    data[dataIndex + 1] = (byte)(Math.Round(cellColor.Y * (255 - (backgroundColor.Y * 255)) * (cells[index] / (float)settings.states)) + (backgroundColor.Y * 255));
                    data[dataIndex + 2] = (byte)(Math.Round(cellColor.Z * (255 - (backgroundColor.Z * 255)) * (cells[index] / (float)settings.states)) + (backgroundColor.Z * 255));
                    data[dataIndex + 3] = 255;
                }
            }

            texture.Update(data);
        }

        private int GetIndex(int x, int y)
        {
            return y * width + x;
        }

        private byte GetCell(int x, int y)
        {
            while (x < 0)
            {
                x += width;
            }

            while (y < 0)
            {
                y += height;
            }

            while (x >= width)
            {
                x -= width;
            }

            while (y >= height)
            {
                y -= height;
            }

            return cells[GetIndex(x, y)];
        }
    }
}
