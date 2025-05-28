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
        public VictimData Data { get; set; }

        public VictimPed(Ped ped, VictimType type = VictimType.NORMAL_PED, VictimData victimData = null)
        {
            Ped = ped ?? throw new ArgumentNullException(nameof(ped));
            Type = type;
            Data = victimData;
        }

        public VictimPed(Ped ped, VictimData victimData = null)
        {
            Ped = ped ?? throw new ArgumentNullException(nameof(ped));
            Type = PedUtil.GetVictimType(victimData);
            Data = victimData;
        }

        public VictimPed(VictimData victimData)
        {
            Ped = victimData.Ped ?? throw new ArgumentNullException(nameof(victimData.Ped));
            Type = PedUtil.GetVictimType(victimData);
            Data = victimData;
        }
    }
}
