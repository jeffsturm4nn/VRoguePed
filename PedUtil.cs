using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRoguePed
{
    internal class PedUtil
    {
        public static List<Ped> GetNearestValidPeds(Entity target, int pedCount)
        {
            var worldPeds = World.GetAllPeds().ToList();
            var nearestPeds = new List<Ped>();

            var sortedPedsByDistance = worldPeds.
                Where(ped => ped != null
                    && ped.Exists()
                    && !ped.IsRagdoll
                    && ped.IsAlive
                    && ped.IsHuman
                    && ped.IsOnFoot
                    && ped != Game.Player.Character).
                OrderBy(ped => Vector3.Distance(ped.Position, target.Position)).
                Take(pedCount);

            nearestPeds.AddRange(sortedPedsByDistance);

            return null;
        }
    }
}
