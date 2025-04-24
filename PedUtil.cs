using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using static VRoguePed.Constants;

namespace VRoguePed
{
    internal class PedUtil
    {
        public static bool IsPedWanderingAround(Ped ped)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, TASK_HASH_WANDERING_AROUND);
        }

        public static void PreventPedFromFleeing(Ped ped)
        {
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 0, true);
        }

        public static void DisposePed(Ped ped)
        {
            if(ped != null && ped.Exists())
            {
                ped.IsPersistent = false;
                ped.MarkAsNoLongerNeeded();
            }
        }

        public static void MakePedCombatResilient(Ped ped)
        {
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 2, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 5, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 13, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 58, true);

            Function.Call(Hash.SET_PED_DESIRED_MOVE_BLEND_RATIO, ped.Handle, 3.0f);
            Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped.Handle, 5.0f);

            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 0, false);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 1, false);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 2, false);

            Function.Call(Hash.SET_PED_COMBAT_ABILITY, ped.Handle, 100);
        }

        public static Blip AttachBlipToPed(Ped ped, BlipColor blipColor, int number)
        {
            Blip blip = ped.AddBlip();
            blip.Color = blipColor;
            blip.ShowNumber(number);

            return blip;
        }

        public static void DeletePedBlip(Blip blip)
        {
            if(blip != null && blip.Exists())
            {
                blip.Remove();
            }
        }

        public static void PerformTaskSequence(Ped ped, Action<TaskSequence> taskAction)
        {
            TaskSequence taskSequence = new TaskSequence();
            taskAction(taskSequence);
            taskSequence.Close();
            ped.Task.PerformSequence(taskSequence);
            taskSequence.Dispose();
        }

        public static List<Ped> GetNearestValidRoguePeds2(Ped target, int pedCount, float maxRadius = 40f, List<RoguePed> ignoreList = null)
        {
            var nearestPeds = new List<Ped>();

            if (target != null && target.Exists())
            {
                var sortedPedsByDistance = World.GetNearbyPeds(target, maxRadius).
                    Where(ped => (ped != null
                        && ped.Exists()
                        && !ped.IsRagdoll
                        && ped.IsAlive
                        && ped.IsHuman
                        && (ped.IsOnFoot || (ped.IsInVehicle() && !ped.IsInFlyingVehicle))
                        && ped != Game.Player.Character)
                        && !(ignoreList != null && ignoreList.Where(rp => rp.IsValid() && rp.Ped == ped).Count() != 0)).
                    OrderBy(ped => Math.Abs(ped.Position.DistanceTo(Game.Player.Character.Position))).
                    Take(pedCount);

                nearestPeds.AddRange(sortedPedsByDistance);
            }

            return nearestPeds;
        }

        public static List<Ped> GetNearestValidRoguePeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            var nearestPeds = new List<Ped>();

            if (target != null && target.Exists())
            {
                var sortedPedsByDistance = World.GetNearbyPeds(target, maxRadius).
                    Where(ped => (ped != null
                        && ped.Exists()
                        && !ped.IsRagdoll
                        && ped.IsAlive
                        && ped.IsHuman
                        && (ped.IsOnFoot || (ped.IsInVehicle() && !ped.IsInFlyingVehicle))
                        && ped != Game.Player.Character)
                        && !(ignoreList != null && ignoreList.Contains(ped))).
                    OrderBy(ped => Math.Abs(ped.Position.DistanceTo(Game.Player.Character.Position))).
                    Take(pedCount);

                nearestPeds.AddRange(sortedPedsByDistance);
            }

            return nearestPeds;
        }

        public static List<Ped> GetNearestValidVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            var nearestPeds = new List<Ped>();

            if (target != null && target.Exists())
            {
                var sortedPedsByDistance = World.GetNearbyPeds(target, maxRadius).
                    Where(ped => (ped != null
                        && ped.Exists()
                        && ped != target
                        && ped.IsAlive
                        && ped.IsHuman
                        && (ped.IsOnFoot || (ped.IsInVehicle() && !ped.IsInFlyingVehicle && ped.CurrentVehicle != Game.Player.Character.CurrentVehicle))
                        && ped != Game.Player.Character)
                        && !(ignoreList != null && ignoreList.Contains(ped))).
                    OrderBy(ped => Math.Abs(ped.Position.DistanceTo(Game.Player.Character.Position))).
                    Take(pedCount);

                nearestPeds.AddRange(sortedPedsByDistance);
            }

            return nearestPeds;
        }

        public static List<Ped> GetNearestPrioritizedValidVictimPeds2(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            var nearestPeds = new List<Ped>();

            if (target != null && target.Exists())
            {
                var sortedPedsByDistance = World.GetNearbyPeds(target, maxRadius)
                    .Where(ped =>
                        ped != null &&
                        ped.Exists() &&
                        ped != target &&
                        ped != Game.Player.Character &&
                        ped.IsHuman &&
                        !ped.IsDead &&
                        (ped.IsOnFoot || (ped.IsInVehicle() && !ped.IsInFlyingVehicle && ped.CurrentVehicle != Game.Player.Character.CurrentVehicle)) &&
                        !(ignoreList != null && ignoreList.Contains(ped)))
                .Select(p => new
                {
                    Ped = p,
                    Distance = p.Position.DistanceTo(target.Position),
                    IsCop = IsCop(p),
                    IsAttackingTarget = p.IsInCombatAgainst(target),
                    IsAttackingPlayer = p.IsInCombatAgainst(Game.Player.Character)
                })
                .OrderBy(p => p.IsCop ? 0 : 1) // cops first
                .OrderBy(p => p.IsAttackingTarget ? 0 : 1) // attackers first
                .OrderBy(p => p.IsAttackingPlayer ? 0 : 1) // attackers first
                .ThenBy(p => p.Distance)       // closest to farthest
                .Select(p => p.Ped)
                .Take(pedCount);

                nearestPeds.AddRange(sortedPedsByDistance);
            }

            return nearestPeds;
        }

        public static List<VictimPed> GetNearestPrioritizedValidVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            var nearestPeds = new List<VictimPed>();

            if (target != null && target.Exists())
            {
                var sortedPedsByDistance = World.GetNearbyPeds(target, maxRadius)
                    .Where(ped =>
                        ped != null &&
                        ped.Exists() &&
                        ped != target &&
                        ped != Game.Player.Character &&
                        ped.IsHuman &&
                        !ped.IsDead &&
                        (ped.IsOnFoot || (ped.IsInVehicle() && !ped.IsInFlyingVehicle && ped.CurrentVehicle != Game.Player.Character.CurrentVehicle)) &&
                        !(ignoreList != null && ignoreList.Contains(ped)))
                .Select(p => new
                {
                    Ped = p,
                    Distance = p.Position.DistanceTo(target.Position),
                    IsCop = IsCop(p),
                    IsAttackingTarget = p.IsInCombatAgainst(target),
                })
                .OrderBy(p => p.IsCop ? 0 : 1) // cops first
                .OrderBy(p => p.IsAttackingTarget ? 0 : 1) // attackers first
                .ThenBy(p => p.Distance)       // closest to farthest
                .Select(p => new VictimPed(p.Ped, GetVictimPedType(p.Ped, target)))
                .Take(pedCount);

                nearestPeds.AddRange(sortedPedsByDistance);
            }

            return nearestPeds;
        }

        public static List<VictimPed> GetPedAttackers(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null, VictimType pedType = VictimType.NORMAL_PED)
        {
            var attackerPeds = new List<VictimPed>();

            if (target != null && target.Exists())
            {
                var sortedPedsByDistance = World.GetNearbyPeds(target, maxRadius)
                    .Where(ped =>
                        ped != null &&
                        ped.Exists() &&
                        ped != target &&
                        ped.IsHuman &&
                        !ped.IsDead &&
                        ped.IsInCombatAgainst(target) &&
                        !(ignoreList != null && ignoreList.Contains(ped)))
                .OrderBy(p => p.Position.DistanceTo(target.Position))
                .Select(p => new VictimPed(p, pedType))
                .Take(pedCount);

                attackerPeds.AddRange(sortedPedsByDistance);
            }

            return attackerPeds;
        }

        public static List<VictimPed> GetNextVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            var playerAttackers = GetPedAttackers(Game.Player.Character, pedCount, maxRadius, ignoreList, VictimType.PLAYER_ATTACKER);

            if(playerAttackers.Count == 0)
            {
                return GetNearestPrioritizedValidVictimPeds(target, pedCount, maxRadius, ignoreList);
            }

            return playerAttackers;
        }

        public static VictimType GetVictimPedType(Ped victimPed, Ped roguePed = null)
        {
            if (victimPed.IsInCombatAgainst(Game.Player.Character))
            {
                return VictimType.PLAYER_ATTACKER;
            }
            else if (Util.IsValid(roguePed) && victimPed.IsInCombatAgainst(roguePed))
            {
                return VictimType.ROGUE_PED_ATTACKER;
            }
            else
            {
                return VictimType.NORMAL_PED;
            }
        }

        //public static List<VictimPed> GetNextVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        //{
        //    List<VictimPed> victimPeds = new List<VictimPed>();
        //    List<Ped> nearbyVictimPeds = GetNearestPrioritizedValidVictimPeds(target, pedCount, maxRadius, ignoreList);

        //    foreach(Ped nearbyPed in nearbyVictimPeds)
        //    {
        //        victimPeds.Add(new VictimPed(nearbyPed, GetVictimPedType(nearbyPed, target)));
        //    }

        //    return victimPeds;
        //}

        private static bool IsCop(Ped ped)
        {
            string modelName = ped.Model.ToString().ToLower();
            return modelName.Contains("cop") || modelName.Contains("sheriff") || modelName.Contains("csb_cop");
        }
    }
}
