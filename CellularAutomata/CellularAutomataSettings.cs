using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellularAutomata
{
    internal struct CellularAutomataSettings
    {
        public byte[] survive;
        public byte[] born;
        public byte states;
        public NeighbourhoodMode neighbourhoodMode;
        public string rule;

        public CellularAutomataSettings(byte[] survive, byte[] born, byte states, NeighbourhoodMode neighbourhoodMode, string rule)
        {
            this.survive = survive;
            this.born = born;
            this.states = states;
            this.neighbourhoodMode = neighbourhoodMode;
            this.rule = rule;
        }
    }

    internal enum NeighbourhoodMode
    {
        Moore,
        VonNeuman
    }
}
