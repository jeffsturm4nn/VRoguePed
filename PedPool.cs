using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;
using GTA.Native;

namespace VRoguePed
{
    internal class PedPool
    {
        private static List<Ped> StoredPeds = new List<Ped>();
        private static int UpdateTimeInMs = 1000000;

        private const int FetchWorldPedsIntervalInMs = 200;
        private const int MaxPedSearchRadius = 240;

        public static List<Ped> GetStoredPeds()
        {
            return StoredPeds;
        }

        public static void StepUpdateTime(int timeIntervalInMs)
        {
            UpdateTimeInMs += timeIntervalInMs;

            if (UpdateTimeInMs >= FetchWorldPedsIntervalInMs)
            {
                UpdateTimeInMs = 0;

                //StoredPeds = World.GetAllPeds().ToList();
                StoredPeds = World.GetNearbyPeds(Game.Player.Character, MaxPedSearchRadius).ToList();
            }
        }
    }
}
