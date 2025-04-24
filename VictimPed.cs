using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;

namespace VRoguePed
{
    internal class VictimPed
    {
        public Ped Ped { get; set; }
        public VictimType Type { get; set; }

        public VictimPed(Ped ped, VictimType type = VictimType.NORMAL_PED)
        {
            Ped = ped ?? throw new ArgumentNullException(nameof(ped));
            Type = type;
        }
    }
}
