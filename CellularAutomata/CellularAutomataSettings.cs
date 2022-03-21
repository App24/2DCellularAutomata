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

        public CellularAutomataSettings(byte[] survive, byte[] born, byte states, NeighbourhoodMode neighbourhoodMode)
        {
            this.survive = survive;
            this.born = born;
            this.states = states;
            this.neighbourhoodMode = neighbourhoodMode;
        }
    }

    internal enum NeighbourhoodMode
    {
        Moore,
        VonNeuman
    }
}
