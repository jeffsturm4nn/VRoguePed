using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms.VisualStyles;

namespace VRoguePed
{
    internal class PedModule
    {
        private static readonly Vector3 AirportEntracePosition = new Vector3(-1337.0f, -3044.0f, 13.9f);
        private static readonly Vector3 LandActWaterReservoirPosition = new Vector3(2150.0f, 5150f, 0f);
        private static readonly Vector3 FranklingHouse1Position = new Vector3(-14f, -1458f, 30f);

        private static int FriendlyRoguePedsGroupHash = -1;
        private static volatile bool hasReachedInterval = false;
        private static int timeCounter = 0;

        public static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;
        public static int RoguePedHealth = 500;
        public static int RoguePedLifetimeInSeconds = 300;
        public static float MaxRoguePedRecruitDistance = 40f;
        public static float MaxVictimPedSearchDistance = 40f;
        public static float MaxRoguePedDistanceFromPlayer = 60f;
        public static float MinVictimPedDistanceToShoot = 19f;
        public static float MaxRoguePedWanderDistance = 30f;
        public static float MaxRoguePedDistanceBeforeDisband = 80f;

        public static void InitPedRelationshipGroups()
        {
            FriendlyRoguePedsGroupHash = World.AddRelationshipGroup("FriendlyRoguePeds");
            World.SetRelationshipBetweenGroups(Relationship.Respect, FriendlyRoguePedsGroupHash, Game.Player.Character.RelationshipGroup);
        }

        public static void AddPedToFriendlyGroup(Ped ped)
        {
            ped.RelationshipGroup = FriendlyRoguePedsGroupHash;
        }

        public static void CheckValidRoguePeds()
        {
            try
            {
                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null)
                    {
                        if (!roguePed.IsValid() ||
                            roguePed.Ped.Position.DistanceTo(Game.Player.Character.Position) > MaxRoguePedDistanceBeforeDisband)
                        {
                            Core.RoguePeds.Remove(roguePed);
                            Core.ProcessedPeds.Remove(roguePed.Ped);

                            if (roguePed.HasValidVictim())
                            {
                                Core.ProcessedPeds.Remove(roguePed.Victim.Ped);
                            }

                            PedUtil.DeletePedBlip(roguePed.Blip);
                            PedUtil.DisposePed(roguePed.Ped);
                        }
                        else if (roguePed.State != RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            bool removeVictimPed = false;

                            if (roguePed.HasValidVictim())
                            {
                                if (roguePed.Victim.Type == VictimType.NORMAL_PED &&
                                    roguePed.DistanceFromVictim() >= MaxVictimPedSearchDistance)
                                {
                                    removeVictimPed = true;
                                    roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                                }
                                else if (roguePed.Victim.Type != VictimType.NORMAL_PED &&
                                    roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                                {
                                    removeVictimPed = true;
                                    roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                                }
                            }

                            if (removeVictimPed)
                            {
                                Core.ProcessedPeds.Remove(roguePed.Victim.Ped);
                                roguePed.Victim = null;
                            }
                        }
                    }
                    else
                    {
                        Core.RoguePeds.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ProcessPeds() Error:\n " + e.ToString());
            }
        }

        public static void UpdateRoguePedsState()
        {
            try
            {
                const int timeInterval = 100 / Constants.UPDATE_INTERVAL;
                bool hasReachedInterval = false;

                if(++timeCounter >= timeInterval)
                {
                    timeCounter = 0;
                    hasReachedInterval = true;
                }

                string sub = "RoguePeds.Count = " + Core.RoguePeds.Count;
                sub += "\nProcessedPeds.Count = " + Core.ProcessedPeds.Count;

                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null && roguePed.IsValid())
                    {
                        sub += "\n[" + i + "]: TaskSeq(" + roguePed.Ped.TaskSequenceProgress +
                            "), Dist(" + roguePed.DistanceFromPlayer() +
                            "), State(" + roguePed.State.ToString() +
                            "), Victim(" + (!Util.IsValid(roguePed.Victim) ? "None)" :
                            ("Found @ " + roguePed.DistanceFromVictim() +
                            ")"));

                        if (roguePed.Ped.TaskSequenceProgress == Constants.TASK_SEQUENCE_IN_PROGRESS)
                        {
                            continue;
                        }
                        else if (roguePed.State == RogueState.LOOKING_FOR_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (hasReachedInterval)
                            {
                                if (Util.IsValid(roguePed.Victim))
                                {
                                    Core.ProcessedPeds.Remove(roguePed.Victim.Ped);
                                    roguePed.Victim = null;
                                }

                                VictimPed victim = PedUtil.GetNextVictimPeds(roguePed.Ped, 1, MaxVictimPedSearchDistance, Core.ProcessedPeds).FirstOrDefault();

                                if (Util.IsValid(victim))
                                {
                                    roguePed.Victim = victim;
                                    roguePed.State = RogueState.RUNNING_TOWARDS_VICTIM;
                                }
                                else if (!PedUtil.IsPedWanderingAround(roguePed.Ped))
                                //else if (!roguePed.Ped.IsWalking)
                                {
                                    roguePed.Ped.Task.WanderAround(Game.Player.Character.Position, MaxRoguePedWanderDistance);
                                }

                                roguePed.Ped.WetnessHeight = 2f;
                            }
                        }
                        else if (roguePed.State == RogueState.RUNNING_TOWARDS_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (roguePed.HasValidVictim())
                            {
                                if (roguePed.DistanceFromVictim() <= MinVictimPedDistanceToShoot)
                                {
                                    roguePed.State = RogueState.ATTACKING_VICTIM;
                                }
                                else if (hasReachedInterval)
                                {
                                    if (!roguePed.Ped.IsRunning)
                                    {
                                        roguePed.Ped.Task.RunTo(roguePed.Victim.Ped.Position, false); 
                                    }
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                        else if (roguePed.State == RogueState.ATTACKING_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (roguePed.HasValidVictim())
                            {
                                if (roguePed.DistanceFromVictim() <= MinVictimPedDistanceToShoot)
                                {
                                    if (hasReachedInterval)
                                    {
                                        if (!roguePed.Victim.Ped.IsInVehicle())
                                        {
                                            if (!roguePed.Ped.IsInCombatAgainst(roguePed.Victim.Ped))
                                            {
                                                PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.FightAgainst(roguePed.Victim.Ped); });
                                            }
                                        }
                                        else
                                        {
                                            roguePed.Ped.Task.ShootAt(roguePed.Victim.Ped, 5000, FiringPattern.FullAuto);
                                        }
                                    }
                                }
                                else
                                {
                                    roguePed.State = RogueState.RUNNING_TOWARDS_VICTIM;
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                        else if (roguePed.State == RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                if (hasReachedInterval)
                                {
                                    roguePed.Ped.Task.RunTo(Game.Player.Character.Position, false);
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                    }
                }

                //UI.ShowSubtitle(sub);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed UpdateRoguePedsState() Error:\n " + e.ToString());
            }
        }

        public static void MakePedGoRogueProc(bool targetedVictimPed, bool targetedRoguePed)
        {
            MakePedGoRogue(targetedVictimPed, targetedRoguePed);
        }

        private static Ped RecruitRoguePed(bool targetedRoguePed)
        {
            Ped roguePed = null;

            try
            {
                if (targetedRoguePed && Game.Player.IsAlive && Game.Player.IsAiming)
                {
                    Entity targetEntity = null;
                    Util.GetEntityPlayerIsAimingAt(ref targetEntity);

                    if (targetEntity != null && targetEntity.Exists() && Util.IsNPCPed(targetEntity)
                        && !Core.ProcessedPeds.Contains((Ped)targetEntity))
                    {
                        roguePed = (Ped)targetEntity;
                    }
                }
                else
                {
                    roguePed = PedUtil.GetNearestValidRoguePeds(Game.Player.Character, 1, 40f, Core.ProcessedPeds).FirstOrDefault();
                }

            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed RecruitRoguePed() Error:\n" + e.ToString(), true);
            }

            return roguePed;
        }

        public static void MakePedGoRogue(bool targetedVictimPed, bool targetedRoguePed)
        {
            try
            {
                Ped roguePed = RecruitRoguePed(targetedRoguePed);

                if (roguePed == null || !roguePed.Exists())
                {
                    return;
                }

                VictimPed victimPed = null;

                if (targetedVictimPed && Game.Player.IsAlive && Game.Player.IsAiming)
                {
                    Entity targetEntity = null;
                    Util.GetEntityPlayerIsAimingAt(ref targetEntity);

                    if (targetEntity != null && targetEntity.Exists() && Util.IsNPCPed(targetEntity)
                        && !Core.ProcessedPeds.Contains((Ped)targetEntity))
                    {
                        victimPed = new VictimPed(((Ped)targetEntity), VictimType.PLAYER_TARGET);
                    }
                }

                PedUtil.SetRoguePedParameters(roguePed);
                PedUtil.MakePedCombatResilient(roguePed);

                VehicleSeat playerVehicleSeat = VehicleSeat.None;

                if (roguePed.IsInVehicle())
                {
                    if (roguePed.CurrentVehicle == Game.Player.Character.CurrentVehicle)
                    {
                        playerVehicleSeat = VehicleUtil.GetSeatPedIsSittingOn(roguePed, Game.Player.Character.CurrentVehicle);
                    }
                }

                if (playerVehicleSeat == VehicleSeat.None)
                {
                    playerVehicleSeat = VehicleUtil.GetPlayerVehicleFreeSeat();
                }

                RoguePed currentRoguePed = Core.RoguePeds.Where(p => p.Ped.Equals(roguePed)).FirstOrDefault();

                if (currentRoguePed == null || !currentRoguePed.IsValid())
                {
                    Blip blip = PedUtil.AttachBlipToPed(roguePed, BlipColor.BlueLight, (Core.RoguePeds.Count + 1));
                    Core.RoguePeds.Add(new RoguePed(roguePed, victimPed, playerVehicleSeat, blip, RoguePedLifetimeInSeconds * 1000));
                    Core.ProcessedPeds.Add(roguePed);
                    AddPedToFriendlyGroup(roguePed);
                }
                else
                {
                    currentRoguePed.PlayerVehicleSeat = playerVehicleSeat;
                    currentRoguePed.Victim = victimPed;
                }

                if (Util.IsValid(victimPed))
                {
                    Core.ProcessedPeds.Add(victimPed.Ped);
                }
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed MakePedGoRogueProc() Error:\n" + e.ToString(), true);
            }
        }

        public static void ReadPedParamsFromConfig(ScriptSettings settings)
        {
            try
            {
                RoguePedHealth = settings.GetValue<int>("PED_PARAMETERS", "RoguePedHealth", 1000);
                String weaponName = settings.GetValue("PED_PARAMETERS", "RoguePedWeaponName", "Pistol50");

                if (weaponName == null || weaponName.Length == 0 ||
                    weaponName.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                {
                    weaponName = "Unarmed";
                }

                RoguePedWeaponHash = (WeaponHash)Enum.Parse(typeof(WeaponHash), weaponName);

                MaxRoguePedRecruitDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedRecruitDistance", 40);
                MaxVictimPedSearchDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxVictimPedSearchDistance", 40);
                MaxRoguePedDistanceFromPlayer = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedDistanceFromPlayer", 60);
                MinVictimPedDistanceToShoot = settings.GetValue<int>("PED_PARAMETERS", "MinVictimPedDistanceToShoot", 19);
                MaxRoguePedWanderDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedWanderDistance", 30);
                MaxRoguePedDistanceBeforeDisband = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedDistanceBeforeDisband", 80);
                RoguePedLifetimeInSeconds = settings.GetValue<int>("PED_PARAMETERS", "RoguePedLifetimeInSeconds", 300);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ReadPedParamsFromConfig() Error:\n " + e.ToString());
            }
        }
    }
}
