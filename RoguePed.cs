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
            PlayerVehicleSeat = playerVehicleSeat;
            State = RogueState.LOOKING_FOR_VICTIM;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public RoguePed(Ped ped, Ped victim, VehicleSeat playerVehicleSeat = VehicleSeat.None)
        {
            Ped = ped;
            Victim = victim;
            PlayerVehicleSeat = playerVehicleSeat;
            State = RogueState.LOOKING_FOR_VICTIM;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public Ped Ped { get; set; }
        public VehicleSeat PlayerVehicleSeat { get; set; }

        public Ped Victim { get; set; }

        public RogueState State { get; set; }

        public bool IsValid()
        {
            return (Ped != null && Ped.Exists() && !Ped.IsDead);
        }

        public bool HasValidVictim()
        {
            return (Util.IsValid(Victim) && Victim.IsAlive);
        }

        public float DistanceFromVictim()
        {
            if(IsValid() && HasValidVictim())
            {
                return (Ped.Position.DistanceTo(Victim.Position));
            }

            return float.MaxValue - 1;
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
                ", Victims=" + (Victim != null ? Victim.ToString() : "null") +
                ", PlayerVehicleSeat=" + Enum.GetName(typeof(VehicleSeat), PlayerVehicleSeat) ;
        }
    }
}
