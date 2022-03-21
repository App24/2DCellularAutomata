﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellularAutomata
{
    internal class CellularAutomataSimulation
    {
        public static uint SimulationSpeed { get; set; }

        private readonly int width;
        private readonly int height;
        private readonly CellularAutomataSettings settings;

        private byte[] cells;

        public readonly Texture texture;

        public CellularAutomataSimulation(int width, int height, string settings) : this(width, height, Utils.GenerateSettings(settings))
        {

        }

        public CellularAutomataSimulation(int width, int height, CellularAutomataSettings settings)
        {
            this.width = width;
            this.height = height;
            this.settings = settings;
            cells = new byte[width * height];
            Random random = new Random();
            //cells[GetIndex((int)(width / 2f), (int)(height / 2f))] = settings.states;
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
            texture = new Texture((uint)width, (uint)height);

            UpdateTexture();
        }

        public Texture Simulate()
        {
            byte[] updateCells = new byte[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                updateCells[i] = cells[i];
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    updateCells[GetIndex(x, y)] = SimulateCell(x, y);
                }
            }
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
                    data[dataIndex + 0] = (byte)Math.Round(255 * (cells[index] / (float)settings.states));
                    data[dataIndex + 1] = (byte)Math.Round(255 * (cells[index] / (float)settings.states));
                    data[dataIndex + 2] = (byte)Math.Round(255 * (cells[index] / (float)settings.states));
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
