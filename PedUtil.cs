using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using static VRoguePed.Constants;
using static VRoguePed.PedModule;

namespace VRoguePed
{
    internal class PedUtil
    {
        private static List<Ped> NearbyPeds = new List<Ped>(300);
        private static List<Ped> ValidPeds = new List<Ped>(300);
        private static List<VictimPed> VictimPeds = new List<VictimPed>(300);

        public static int Count(Ped ped, List<Ped> pedList)
        {
            int count = 0;

            if (ped == null || pedList == null)
            {
                return 0;
            }

            for (int i = 0; i < pedList.Count; i++)
            {
                if (pedList[i] != null && pedList[i] == ped)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsPedWanderingAround(Ped ped)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, TASK_HASH_WANDERING_AROUND);
        }

        public static bool IsRoguePed(Ped ped)
        {
            return Core.RoguePedsMap.ContainsKey(ped.Handle);
        }

        public static bool IsProcessedPed(Ped ped)
        {
            return Core.ProcessedPeds.Where(pp => pp == ped).Any();
        }

        public static void UpdateProcessedPedCount(Ped ped, int increment)
        {
            if (ped != null)
            {
                if (Core.ProcessedPedCountMap.TryGetValue(ped.Handle, out var count))
                {
                    count += increment;
                    Core.ProcessedPedCountMap[ped.Handle] = count;

                    if (count <= 0)
                    {
                        Core.ProcessedPedCountMap.Remove(ped.Handle);
                    }
                }
                else
                {
                    Core.ProcessedPedCountMap.Add(ped.Handle, increment);
                }
            }
        }

        public static int ProcessedPedCount(Ped ped)
        {
            if (ped != null)
            {
                if (Core.ProcessedPedCountMap.TryGetValue(ped.Handle, out var count))
                {
                    return count ?? 0;
                }
            }

            return 0;
        }

        public static void DisposePed(Ped ped)
        {
            if (ped != null && ped.Exists())
            {
                ped.IsPersistent = false;
                //ped.MarkAsNoLongerNeeded();
            }
        }

        public static void MakePedCombatResilient(Ped ped)
        {
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 0, false);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 11, false);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 12, false);

            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 2, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 5, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 13, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 50, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, 58, true);

            //Function.Call(Hash.SET_PED_DESIRED_MOVE_BLEND_RATIO, ped.Handle, 3.0f);
            //Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped.Handle, 5.0f);

            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 0, false);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 1, false);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 2, false);

            Function.Call(Hash.SET_PED_COMBAT_ABILITY, ped.Handle, 100);


        }

        public static void SetRoguePedParameters(Ped roguePed)
        {
            roguePed.IsPersistent = true;
            roguePed.Weapons.Give(WeaponHash.Hatchet, 1, false, true);
            roguePed.Weapons.Give(RoguePedWeaponHash, 99999, true, true);
            roguePed.MaxHealth = RoguePedHealth;
            roguePed.Health = RoguePedHealth;
            roguePed.CanRagdoll = false;
            roguePed.CanSufferCriticalHits = false;
            roguePed.CanWrithe = false;
            roguePed.MaxSpeed = 100f;
            roguePed.WetnessHeight = 6f;
            roguePed.AlwaysKeepTask = true;
            roguePed.BlockPermanentEvents = true;
            roguePed.FiringPattern = FiringPattern.FullAuto;
            roguePed.CanSwitchWeapons = true;
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
            if (blip != null && blip.Exists())
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

        public static List<Ped> GetNearestValidRoguePeds(int pedCount, float maxRecruitRadius, List<Ped> ignoreList = null)
        {
            return PedPool.GetStoredPeds().
                Where(ped => (Util.IsValid(ped)
                    && ped.IsHuman
                    && ped.IsAlive
                    && ped != Game.Player.Character)
                    && !ped.IsRagdoll
                    && (ped.IsOnFoot || (ped.IsInVehicle() && !ped.IsInFlyingVehicle))
                    && (ped.Position.DistanceTo(Game.Player.Character.Position) <= maxRecruitRadius)
                    && !(ignoreList != null && ignoreList.Contains(ped))).
                OrderBy(ped => ped.Position.DistanceTo(Game.Player.Character.Position)).
                Take(pedCount).
                ToList();
        }

        public static int GetVictimTargetPriority(VictimData victimData)
        {
            if (victimData.IsAttackingPlayer)
            {
                return -500;
            }
            else if (victimData.IsAttackingTarget)
            {
                return 0;
            }
            else if (victimData.AttackedRoguePed != null || victimData.IsAttackingOtherRoguePeds 
                || victimData.IsAttackingTarget)
            {
                return 500;
            }
            else if (victimData.IsCop)
            {
                return 1000;
            }
            else
            {
                return 1500;
            }
        }

        //public static List<VictimPed> GetNearestPrioritizedValidVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        //{
        //    ValidPeds.Clear();
        //    NearbyPeds.Clear();
        //    VictimPeds.Clear();

        //    if (target != null && target.Exists())
        //    {
        //        List<Ped> allPeds = PedPool.GetStoredPeds();
        //        List<VictimData> victimDataList = Core.VictimPedData;

        //        for (int i = 0; i < allPeds.Count; i++)
        //        {
        //            Ped ped = allPeds[i];

        //            if (Util.IsValid(ped) && ped != target && ped != Game.Player.Character && ped.IsHuman &&
        //                !ped.IsDead && ped.RelationshipGroup != FriendlyRoguePedsGroupHash && !IsRoguePed(ped) &&
        //                !(ped.IsInVehicle() && ped.CurrentVehicle == Game.Player.Character.CurrentVehicle))
        //            {
        //                NearbyPeds.Add(ped);
        //            }
        //        }

        //        for (int i = 0; i < NearbyPeds.Count; i++)
        //        {
        //            Ped ped = NearbyPeds[i];

        //            if (!(ignoreList != null && Count(ped, ignoreList) >= MaxRoguePedsPerTarget))
        //            {
        //                ValidPeds.Add(ped);
        //            }
        //        }

        //        for (int i = 0; i < ValidPeds.Count; i++)
        //        {
        //            Ped ped = ValidPeds[i];

        //            VictimData vd = new VictimData
        //            {
        //                Ped = ped,
        //                Distance = Distance(target, ped),
        //                IsCop = IsCop(ped),
        //                IsAttackingOtherRoguePeds = HasAttackedRoguePeds(ped, target, true),
        //                IsAttackingTarget = (ped.IsInCombatAgainst(target)),
        //                IsAttackingPlayer = ped.IsInCombatAgainst(Game.Player.Character)
        //            };

        //            if (!(RoguePedsBodyguardMode &&
        //                !(vd.IsAttackingPlayer || vd.IsAttackingTarget || vd.IsAttackingOtherRoguePeds)))
        //            {
        //                VictimPeds.Add(new VictimPed(vd.Ped, vd));
        //            }
        //        }

        //        VictimPeds.Sort((v1, v2) =>
        //        {
        //            int tp1 = GetVictimTargetPriority(v1.Data);
        //            int tp2 = GetVictimTargetPriority(v2.Data);
        //            int comparePriority = tp1.CompareTo(tp2);

        //            return (comparePriority != 0 ? comparePriority : v1.Data.Distance.CompareTo(v2.Data.Distance));
        //        });

        //        if (VictimPeds.Count >= pedCount)
        //        {
        //            return VictimPeds.GetRange(0, pedCount);
        //        }

        //        return new List<VictimPed>();

        //        //return PedPool.GetStoredPeds()
        //        //    .Where(ped =>
        //        //        Util.IsValid(ped) &&
        //        //        ped != target &&
        //        //        ped != Game.Player.Character &&
        //        //        ped.IsHuman &&
        //        //        !ped.IsDead &&
        //        //        ped.RelationshipGroup != FriendlyRoguePedsGroupHash &&
        //        //        !IsRoguePed(ped))
        //        //    .Where(ped =>
        //        //        !(ped.IsInVehicle() && ped.CurrentVehicle == Game.Player.Character.CurrentVehicle) &&
        //        //        !(ignoreList != null && ignoreList.Count(ped => ped == ped) >= MaxRoguePedsPerTarget))
        //        //.Select(ped => new VictimData
        //        //{
        //        //    Ped = ped,
        //        //    Distance = Distance(target, ped),
        //        //    IsCop = IsCop(ped),
        //        //    IsAttackingOtherRoguePeds = HasAttackedRoguePeds(ped, target),
        //        //    IsAttackingTarget = (ped.IsInCombatAgainst(target) || HasPedAttackedAnother(target, ped)),
        //        //    IsAttackingPlayer = ped.IsInCombatAgainst(Game.Player.Character)
        //        //})
        //        //.Where(vd => !(RoguePedsBodyguardMode &&
        //        //!(vd.IsAttackingPlayer || vd.IsAttackingTarget || vd.IsAttackingOtherRoguePeds)))
        //        //.OrderBy(vd => GetVictimTargetPriority(vd))
        //        //.ThenBy(vd => vd.Distance)
        //        //.Select(vd => new VictimPed(vd.Ped, GetVictimType(vd)))
        //        //.Take(pedCount)
        //        //.ToList();
        //    }

        //    return new List<VictimPed>();
        //}

        public static List<VictimData> GetCurrentValidVictimPedsData()
        {
            return PedPool.GetStoredPeds()
                .Where(ped =>
                    Util.IsValid(ped) &&
                    ped != Game.Player.Character &&
                    ped.IsHuman &&
                    !ped.IsDead &&
                    ped.RelationshipGroup != FriendlyRoguePedsGroupHash &&
                    ProcessedPedCount(ped) < MaxRoguePedsPerTarget &&
                    !(ped.IsInVehicle() && ped.CurrentVehicle == Game.Player.Character.CurrentVehicle) &&
                    !IsRoguePed(ped))
            .Select(ped => new VictimData
            {
                Ped = ped,
                IsCop = IsCop(ped),
                IsAttackingPlayer = ped.IsInCombatAgainst(Game.Player.Character),
                AttackedRoguePed = GetRoguePedAttackedBy(ped)
            })
            .Where(vd =>
                !(RoguePedsBodyguardMode &&
                !(vd.IsAttackingPlayer || Util.IsValid(vd.AttackedRoguePed)))
            )
            //.OrderBy(vd => GetVictimTargetPriority(vd))
            .ToList();
        }

        public static VictimType GetVictimType(VictimData victimData)
        {
            if (victimData.IsAttackingPlayer)
            {
                return VictimType.PLAYER_ATTACKER;
            }
            else if (victimData.AttackedRoguePed != null || victimData.IsAttackingTarget ||
                victimData.IsAttackingOtherRoguePeds)
            {
                return VictimType.ROGUE_PED_ATTACKER;
            }
            else
            {
                return VictimType.NORMAL_PED;
            }
        }

        private static bool IsCop(Ped ped)
        {
            string modelName = ped.Model.ToString().ToLower();

            return modelName.Contains("cop") || modelName.Contains("sheriff")
                     || modelName.Contains("fib") || modelName.Contains("swat")
                     || modelName.Contains("marine") || modelName.Contains("security")
                      || modelName.Contains("ciasec") || modelName.Contains("blackops")
                       || modelName.Contains("armoured") || modelName.Contains("prisguard");
        }

        public static void RemoveVictim(VictimPed victim)
        {
            if (victim != null)
            {
                UpdateProcessedPedCount(victim.Ped, -1);

                Core.ProcessedPeds.Remove(victim.Ped);
            }
        }

        public static void RemoveRoguePed(RoguePed roguePed)
        {
            if (roguePed != null)
            {
                PedUtil.RemoveVictim(roguePed.Victim);
                PedUtil.DeletePedBlip(roguePed.Blip);

                Core.ProcessedPeds.Remove(roguePed.Ped);
                Core.RoguePeds.Remove(roguePed);

                UpdateProcessedPedCount(roguePed.Ped, -1);

                if (Util.IsValid(roguePed.Ped))
                {
                    Core.RoguePedsMap.Remove(roguePed.Ped.Handle);
                }

                PedUtil.DisposePed(roguePed.Ped);
            }
        }

        public static void ResetRoguePedVictim(RoguePed roguePed, VictimPed newVictimPed)
        {
            if (roguePed != null && newVictimPed != null)
            {
                RemoveVictim(roguePed.Victim);
                InsertVictimPed(newVictimPed);

                roguePed.Victim = newVictimPed;
            }
        }

        public static void InsertVictimPed(VictimPed victimPed)
        {
            if (Util.IsValid(victimPed))
            {
                Core.ProcessedPeds.Add(victimPed.Ped);

                UpdateProcessedPedCount(victimPed.Ped, 1);
            }
        }

        public static void InsertRoguePed(RoguePed roguePed)
        {
            if (Util.IsValid(roguePed))
            {
                Core.ProcessedPeds.Add(roguePed.Ped);

                Core.RoguePeds.Add(roguePed);
                Core.RoguePedsMap.Add(roguePed.Ped.Handle, roguePed);

                UpdateProcessedPedCount(roguePed.Ped, 1);
            }
        }

        public static float Distance(Ped ped1, Ped ped2)
        {
            return ped1.Position.DistanceTo(ped2.Position);
        }

        public static bool IsVictimInAttackingRange(RoguePed roguePed, VictimPed victimPed)
        {
            if (victimPed.Type != VictimType.PLAYER_ATTACKER &&
                                    victimPed.Type != VictimType.PLAYER_TARGET &&
                                    victimPed.Type != VictimType.ROGUE_PED_ATTACKER)
            {
                if ((roguePed.DistanceFromVictim(victimPed) >= MaxVictimPedOnFootChaseDistance) ||
                     ((victimPed.Ped.IsFleeing || victimPed.Ped.Velocity.Length() > 6f)
                     && roguePed.DistanceFromVictim(victimPed) >= MaxVictimPedInVehicleChaseDistance))
                {
                    return false;
                }
            }
            else if (victimPed.Ped.IsFleeing || victimPed.Ped.Velocity.Length() > 6f)
            {
                return false;
            }

            return true;
        }

        public static bool IsPedFatallyInjured(Ped ped)
        {
            return Function.Call<bool>(Hash.IS_PED_FATALLY_INJURED, ped);
        }

        public static bool HasPedAttackedAnother(Ped target, Ped attacker, bool clearAttackerStatus = true)
        {
            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, target.Handle, attacker.Handle, clearAttackerStatus);
        }

        public static RoguePed GetRoguePedAttackedBy(Ped attacker, bool considerCombatAsAttack = true)
        {
            return Core.RoguePeds
                .Where(rp => (considerCombatAsAttack && attacker.IsInCombatAgainst(rp.Ped))
                || HasPedAttackedAnother(rp.Ped, attacker, true))
                .FirstOrDefault();
        }

        public static VictimData GetNearestVictimPed(RoguePed roguePed, List<VictimData> victimDataList)
        {
            return victimDataList
                .Where(vd => ProcessedPedCount(vd.Ped) < MaxRoguePedsPerTarget &&
                vd.Ped != Game.Player.Character &&
                vd.Ped.RelationshipGroup != FriendlyRoguePedsGroupHash &&
                !IsRoguePed(vd.Ped))
                .OrderBy(vd => PedUtil.GetVictimTargetPriority(vd))
                .ThenBy(vd => Distance(roguePed.Ped, vd.Ped))
                .FirstOrDefault();
        }
    }
}
