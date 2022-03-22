using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellularAutomata
{
    internal static class Utils
    {
        public static bool GenerateSettings(string settingsString, out CellularAutomataSettings settings)
        {
            settings = new CellularAutomataSettings();

            string[] separateSettings = settingsString.Split('/');
            if (separateSettings.Length < 4)
                return false;

            settings.rule = settingsString;

            string surviveString = separateSettings[0].Trim();
            string bornString = separateSettings[1].Trim();
            string statesString = separateSettings[2].Trim();
            string neighbourhoodMode = separateSettings[3].Trim().ToLower();

            string[] survives = surviveString.Split(",");

            List<byte> byteSurvives = new List<byte>();

            foreach (string survive in survives)
            {
                if (byte.TryParse(survive.Trim(), out byte byteSurvive))
                {
                    byteSurvives.Add(byteSurvive);
                }
                else
                {
                    string[] bounds = survive.Split("-");
                    if (!byte.TryParse(bounds[0].Trim(), out byte minSurvive) || !byte.TryParse(bounds[1].Trim(), out byte maxSurvive))
                    {
                        return false;
                    }

                    for (int i = minSurvive; i < maxSurvive + 1; i++)
                    {
                        byteSurvives.Add((byte)i);
                    }
                }
            }

            settings.survive = byteSurvives.ToArray();

            string[] borns = bornString.Split(",");

            List<byte> byteBorns = new List<byte>();

            foreach (string born in borns)
            {
                if (byte.TryParse(born.Trim(), out byte byteBorn))
                {
                    byteBorns.Add(byteBorn);
                }
                else
                {
                    string[] bounds = born.Split("-");
                    if (!byte.TryParse(bounds[0].Trim(), out byte minBorn) || !byte.TryParse(bounds[1].Trim(), out byte maxBorn))
                    {
                        return false;
                    }

                    for (int i = minBorn; i < maxBorn + 1; i++)
                    {
                        byteBorns.Add((byte)i);
                    }
                }
            }

            settings.born = byteBorns.ToArray();

            if (!byte.TryParse(statesString, out settings.states))
            {
                return false;
            }

            switch (neighbourhoodMode)
            {
                case "m":
                    {
                        settings.neighbourhoodMode = NeighbourhoodMode.Moore;
                    }
                    break;
                case "n":
                    {
                        settings.neighbourhoodMode = NeighbourhoodMode.VonNeuman;
                    }
                    break;
                default:
                    {
                        return false;
                    }
            }

            return true;
        }
    }
}
