﻿using GTA;
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

        public static List<Ped> GetNearestPrioritizedValidVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
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

        private static bool IsCop(Ped ped)
        {
            string modelName = ped.Model.ToString().ToLower();
            return modelName.Contains("cop") || modelName.Contains("sheriff") || modelName.Contains("csb_cop");
        }

        //private static bool IsAttackingPed(Ped ped, Ped target)
        //{
        //    return (ped.IsShooting || ped.IsInCombatAgainst(target))
        //}
    }
}
