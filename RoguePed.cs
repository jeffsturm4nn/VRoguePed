using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;

namespace VRoguePed
{
    internal class RoguePed
    {
        public RoguePed(Ped ped, VehicleSeat playerVehicleSeat = VehicleSeat.None)
        {
            Ped = ped;
            Victims = new List<Ped>();
            PlayerVehicleSeat = playerVehicleSeat;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public RoguePed(Ped ped, List<Ped> victims, VehicleSeat playerVehicleSeat = VehicleSeat.None)
        {
            Ped = ped;
            Victims = victims;
            PlayerVehicleSeat = playerVehicleSeat;

            if(Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public Ped Ped { get; set; }
        public VehicleSeat PlayerVehicleSeat { get; set; }

        public List<Ped> Victims { get; set; }

        public bool IsValid()
        {
            return (Ped != null && Ped.Exists() && !Ped.IsDead);
        }

        //public static bool operator == (RoguePed p1, RoguePed p2)
        //{
        //    return p1.Ped.Equals(p2.Ped);
        //}

        //public static bool operator !=(RoguePed p1, RoguePed p2)
        //{
        //    return !p1.Ped.Equals(p2.Ped);
        //}

        public override string ToString()
        {
            return "RoguePed{Ped=" + (Ped != null ? Ped.ToString() : "null") +
                ", Victims=" + (Victims != null ? Victims.ToString() : "null") +
                ", PlayerVehicleSeat=" + Enum.GetName(typeof(VehicleSeat), PlayerVehicleSeat) ;
        }
    }
}
