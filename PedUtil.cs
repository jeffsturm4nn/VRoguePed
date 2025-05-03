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
                return World.GetNearbyPeds(target, maxRadius)
                    .Where(ped =>
                        ped != null &&
                        ped.Exists() &&
                        ped != target &&
                        ped != Game.Player.Character &&
                        ped.IsHuman &&
                        !ped.IsDead &&
                        ped.RelationshipGroup != FriendlyRoguePedsGroupHash &&
                        (ped.IsOnFoot || (ped.IsInVehicle() && ped.CurrentVehicle != Game.Player.Character.CurrentVehicle)) &&
                        !(ignoreList != null && ignoreList.Count(p => p == ped) >= MaxRoguePedsPerTarget))
                .Select(p => new
                {
                    Ped = p,
                    Distance = p.Position.DistanceTo(target.Position),
                    IsCop = IsCop(p),
                    IsAttackingTarget = p.IsInCombatAgainst(target),
                })
                .OrderBy(p => p.IsCop ? 0 : 1)
                .OrderBy(p => p.IsAttackingTarget ? 0 : 1)
                .ThenBy(p => p.Distance)
                .Select(p => new VictimPed(p.Ped, GetVictimPedType(p.Ped, target)))
                .Take(pedCount)
                .ToList();
            }

            return new List<VictimPed>();
        }

        public static List<VictimPed> GetPedAttackers(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null, VictimType pedType = VictimType.NORMAL_PED)
        {
            if (target != null && target.Exists())
            {
                return World.GetNearbyPeds(target, maxRadius)
                    .Where(ped =>
                        ped != null &&
                        ped.Exists() &&
                        ped != target &&
                        ped.IsHuman &&
                        !ped.IsDead &&
                        ped.IsInCombatAgainst(target) &&
                        !(ignoreList != null && ignoreList.Count(p => p == ped) >= MaxRoguePedsPerTarget))
                .OrderBy(p => p.Position.DistanceTo(target.Position))
                .Select(p => new VictimPed(p, pedType))
                .Take(pedCount)
                .ToList();
            }

            return new List<VictimPed>();
        }

        public static List<VictimPed> GetNextVictimPeds(Ped target, int pedCount, float maxRadius = 40f, List<Ped> ignoreList = null)
        {
            var playerAttackers = GetPedAttackers(Game.Player.Character, pedCount, maxRadius, ignoreList, VictimType.PLAYER_ATTACKER);

            if (playerAttackers.Count == 0)
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

        public static void ResetRoguePed(RoguePed roguePed)
        {
            int pedIndex = Core.ProcessedPeds.IndexOf(roguePed.Ped);
            Ped originalPed = roguePed.Ped;
            Ped clonedPed = ClonePed(originalPed);

            if (clonedPed != null)
            {
                roguePed.LifetimeInMs = RoguePedLifetimeInSeconds * 1000;

                Core.ProcessedPeds[pedIndex] = roguePed.Ped;
                SetRoguePedParameters(roguePed.Ped);
                originalPed.Delete();

                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
            }
        }

        public static Ped ClonePed(Ped ped)
        {
            try
            {
                float pedHeading = ped.Heading;
                Model pedModel = ped.Model;
                Vector3 pedPosition = ped.Position;

                Ped clonedPed = World.CreatePed(pedModel, pedPosition, pedHeading);

                for (int i = 0; i < 12; i++)
                {
                    int drawable = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped, i);
                    int texture = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped, i);
                    int palette = Function.Call<int>(Hash.GET_PED_PALETTE_VARIATION, ped, i);

                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, clonedPed, i, drawable, texture, palette);
                }

                return clonedPed;
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ClonePed() Error: " + e.Message);
            }

            return null;
        }

        public static void RemoveVictim(VictimPed victim)
        {
            if(victim != null)
            {
                Core.ProcessedPeds.Remove(victim.Ped);
            }
        }

        public static float Distance(Ped ped1, Ped ped2)
        {
            return ped1.Position.DistanceTo(ped2.Position);
        }
    }
}
