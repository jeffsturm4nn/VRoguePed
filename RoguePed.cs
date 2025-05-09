﻿using System;
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
            PlayerVehicleSeat = playerVehicleSeat;
            State = RogueState.LOOKING_FOR_VICTIM;
            Blip = blip;
            LifetimeInMs = lifetimeInMs;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public RoguePed(Ped ped, VictimPed victim, VehicleSeat playerVehicleSeat = VehicleSeat.None, Blip blip = null, int lifetimeInMs = 300 * 1000)
        {
            Ped = ped;
            Victim = victim;
            PlayerVehicleSeat = playerVehicleSeat;
            State = RogueState.LOOKING_FOR_VICTIM;
            Blip = blip;
            LifetimeInMs = lifetimeInMs;

            if (Ped == null)
            {
                throw new NullReferenceException("NULL Ped reference.");
            }
        }

        public Ped Ped { get; set; }
        public VehicleSeat PlayerVehicleSeat { get; set; }

        public VictimPed Victim { get; set; }

        public RogueState State { get; set; }

        public Blip Blip { get; set; }

        public int LifetimeInMs { get; set; }

        public bool IsValid()
        {
            return (Ped != null && Ped.Exists() && Ped.IsAlive);
        }

        public bool HasValidVictim()
        {
            return (Util.IsValid(Victim) && Victim.Ped.IsAlive);
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

        public float DistanceFromPlayer()
        {
            if (IsValid())
            {
                return (Ped.Position.DistanceTo(Game.Player.Character.Position));
            }

            return -1.0f;
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
