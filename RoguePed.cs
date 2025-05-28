using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;

using GTA;

namespace VRoguePed
{
    internal class RoguePed
    {
        public RoguePed(Ped ped, VehicleSeat playerVehicleSeat = VehicleSeat.None, Blip blip = null, int lifetimeInMs = 300*1000)
        {
            Ped = ped;
            IsInUse = true;
            PlayerVehicleSeat = playerVehicleSeat;
            State = RoguePedState.LOOKING_FOR_VICTIM;
            Blip = blip;
            ClearTasksTime = lifetimeInMs;
            TargetCombatDuration = 0;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public RoguePed(Ped ped, VictimPed victim, VehicleSeat playerVehicleSeat = VehicleSeat.None, Blip blip = null, int lifetimeInMs = 300 * 1000)
        {
            Ped = ped;
            IsInUse = true;
            Victim = victim;
            PlayerVehicleSeat = playerVehicleSeat;
            State = RoguePedState.LOOKING_FOR_VICTIM;
            Blip = blip;
            ClearTasksTime = lifetimeInMs;
            TargetCombatDuration = 0;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public Ped Ped { get; set; }

        public bool IsInUse {  get; set; }

        public VehicleSeat PlayerVehicleSeat { get; set; }

        public VictimPed Victim { get; set; }

        public RoguePedState State { get; set; }

        public Blip Blip { get; set; }

        public int ClearTasksTime { get; set; }

        public int TargetCombatDuration { get; set; }

        public bool IsValid()
        {
            return (Ped != null && IsInUse && Ped.Exists() && Ped.IsAlive);
        }

        public bool HasValidVictim()
        {
            return (Util.IsValid(Victim) && Victim.Ped.IsAlive && !PedUtil.IsPedFatallyInjured(Victim.Ped));
        }

        public bool HasBlip()
        {
            return Blip != null && Blip.Exists();
        }

        public float DistanceFromVictim()
        {
            if(IsValid() && HasValidVictim())
            {
                return (Ped.Position.DistanceTo(Victim.Ped.Position));
            }

            return -1.0f;
        }

        public float DistanceFromVictim(VictimPed victimPed)
        {
            if(Util.IsValid(victimPed))
            {
                return (Ped.Position.DistanceTo(victimPed.Ped.Position));
            }

            return -1.0f;
        }
        public float DistanceFromPlayer()
        {
            if (IsValid())
            {
                return (Ped.Position.DistanceTo(Game.Player.Character.Position));
            }

            return -1.0f;
        }


        public bool IsInCombatWithVictim()
        {
            if(IsValid() && HasValidVictim())
            {
                return (Ped.IsInCombatAgainst(Victim.Ped));
            }

            return false;
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
                ", Victim=" + (Victim != null ? Victim.ToString() : "null") +
                ", PlayerVehicleSeat=" + Enum.GetName(typeof(VehicleSeat), PlayerVehicleSeat) + "}" ;
        }
    }
}
