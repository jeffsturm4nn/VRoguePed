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
using static VRoguePed.PedModule;

namespace VRoguePed
{
    internal class PedUtil
    {
        public static bool IsPedWanderingAround(Ped ped)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, TASK_HASH_WANDERING_AROUND);
        }

        public static bool IsRoguePed(Ped ped)
        {
            return Core.RoguePeds.Where(rp => rp.Ped == ped).Any();
        }

        public static bool IsProcessedPed(Ped ped)
        {
            return Core.ProcessedPeds.Where(pp => pp == ped).Any();
        }

        public static void DisposePed(Ped ped)
        {
            if (ped != null && ped.Exists())
            {
                ped.IsPersistent = false;
                ped.MarkAsNoLongerNeeded();
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

            Function.Call(Hash.SET_PED_DESIRED_MOVE_BLEND_RATIO, ped.Handle, 3.0f);
            Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped.Handle, 5.0f);

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
            roguePed.WetnessHeight = 2f;
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

        public static List<VictimPed> GetNearestPrioritizedValidVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            if (target != null && target.Exists())
            {
                return PedPool.GetStoredPeds()
                    .Where(ped =>
                        ped != null &&
                        ped.Exists() &&
                        ped != target &&
                        ped != Game.Player.Character &&
                        ped.IsHuman &&
                        !ped.IsDead &&
                        ped.RelationshipGroup != FriendlyRoguePedsGroupHash &&
                        (ped.IsOnFoot || (ped.IsInVehicle() && ped.CurrentVehicle != Game.Player.Character.CurrentVehicle)) &&
                        !IsRoguePed(ped) &&
                        !(ignoreList != null && ignoreList.Count(p => p == ped) >= MaxRoguePedsPerTarget))
                .Select(p => new VictimData
                {
                    Ped = p,
                    Distance = p.Position.DistanceTo(target.Position),
                    IsCop = IsCop(p),
                    IsAttackingRoguePeds = HasAttackedRoguePeds(p, target),
                    IsAttackingTarget = (p.IsInCombatAgainst(target) || HasPedAttackedAnother(target, p)),
                    IsAttackingPlayer = p.IsInCombatAgainst(Game.Player.Character)
                })
                .OrderBy(vd => vd.IsCop ? 3 : 100)
                .OrderBy(vd => vd.IsAttackingRoguePeds ? 2 : 100)
                .OrderBy(vd => vd.IsAttackingTarget ? 1 : 100)
                .OrderBy(vd => vd.IsAttackingPlayer ? 0 : 100)
                .ThenBy(vd => vd.Distance)
                .Select(vd => new VictimPed(vd.Ped, GetVictimType(vd)))
                .Take(pedCount)
                .ToList();
            }

            return new List<VictimPed>();
        }

        public static List<VictimPed> GetNextVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            return GetNearestPrioritizedValidVictimPeds(target, pedCount, maxRadius, ignoreList);
        }

        public static VictimType GetVictimType(VictimData victimData)
        {
            if (victimData.IsAttackingPlayer)
            {
                return VictimType.PLAYER_ATTACKER;
            }
            else if (victimData.IsAttackingTarget || victimData.IsAttackingRoguePeds)
            {
                return VictimType.ROGUE_PED_ATTACKER;
            }
            else
            {
                return VictimType.NORMAL_PED;
            }
        }

        public static VictimType GetVictimPedType(Ped victimPed, Ped roguePed = null)
        {
            if (victimPed.IsInCombatAgainst(Game.Player.Character))
            {
                return VictimType.PLAYER_ATTACKER;
            }
            else if (Util.IsValid(roguePed) && roguePed != Game.Player.Character
                && victimPed.IsInCombatAgainst(roguePed))
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
                Core.ProcessedPeds.Remove(victim.Ped);
            }
        }

        public static float Distance(Ped ped1, Ped ped2)
        {
            return ped1.Position.DistanceTo(ped2.Position);
        }

        public static bool IsPedFatallyInjured(Ped ped)
        {
            return Function.Call<bool>(Hash.IS_PED_FATALLY_INJURED, ped);
        }

        public static bool HasPedAttackedAnother(Ped target, Ped attacker, bool clearAttackerStatus = true)
        {
            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, target.Handle, attacker.Handle, clearAttackerStatus);
        }

        public static bool HasAttackedRoguePeds(Ped attacker, Ped currentRoguePed = null, bool considerCombatAsAttack = false)
        {
            return Core.RoguePeds
                .Where(rp => !(currentRoguePed != null && rp.Ped == currentRoguePed) &&
                    (HasPedAttackedAnother(rp.Ped, attacker, true) ||
                    (considerCombatAsAttack && attacker.IsInCombatAgainst(rp.Ped)))
                ).Any();
        }
    }
}
