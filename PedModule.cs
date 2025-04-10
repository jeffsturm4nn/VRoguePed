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
        private static Timer Timer = new Timer(800);
        private static volatile bool HasReachedInterval = false;

        private static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;
        private static int RoguePedHealth = 500;
        private static float MaxRoguePedRecruitDistance = 40f;
        private static float MaxVictimPedSearchDistance = 40f;
        private static float MaxRoguePedDistanceFromPlayer = 100f;
        private static float MinVictimPedDistanceToShoot = 19f;
        private static float MaxRoguePedDistanceBeforeDisband = 120f;

        private static void OnTimeElapsed(Object source, ElapsedEventArgs e)
        {
            HasReachedInterval = true;
        }
        public static void InitPedRelationshipGroups()
        {
            FriendlyRoguePedsGroupHash = World.AddRelationshipGroup("FriendlyRoguePeds");
            World.SetRelationshipBetweenGroups(Relationship.Respect, FriendlyRoguePedsGroupHash, Game.Player.Character.RelationshipGroup);
        }

        public static void InitGlobalTimer()
        {
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Timer.Elapsed += OnTimeElapsed;
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
                            Core.ProcessedPeds.Remove(roguePed.Victim);

                            PedUtil.DeletePedBlip(roguePed.Blip);
                            PedUtil.DisposePed(roguePed.Ped);
                        }
                        else if (roguePed.State != RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            if (!Util.IsValid(roguePed.Victim) ||
                                (roguePed.Victim.IsDead || roguePed.DistanceFromVictim() > MaxVictimPedSearchDistance))
                            {
                                Core.ProcessedPeds.Remove(roguePed.Victim);

                                roguePed.Victim = null;
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
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
                        else
                        if (roguePed.State == RogueState.LOOKING_FOR_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (HasReachedInterval)
                            {
                                if (Util.IsValid(roguePed.Victim))
                                {
                                    Core.ProcessedPeds.Remove(roguePed.Victim);
                                    roguePed.Victim = null;
                                }

                                Ped victim = PedUtil.GetNearestPrioritizedValidVictimPeds(roguePed.Ped, 1, MaxVictimPedSearchDistance, Core.ProcessedPeds).FirstOrDefault();

                                if (Util.IsValid(victim))
                                {
                                    roguePed.Victim = victim;
                                    roguePed.State = RogueState.RUNNING_TOWARDS_VICTIM;
                                }
                                //else if (PedUtil.IsPedWanderingAround(roguePed.Ped))
                                else if (!roguePed.Ped.IsWalking)
                                {
                                    roguePed.Ped.Task.WanderAround(Game.Player.Character.Position, (MaxRoguePedDistanceFromPlayer - 10f));
                                }

                                HasReachedInterval = false;
                            }
                        }
                        else if (roguePed.State == RogueState.RUNNING_TOWARDS_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (roguePed.DistanceFromVictim() <= MinVictimPedDistanceToShoot)
                            {
                                roguePed.State = RogueState.SHOOTING_AT_VICTIM;
                            }
                            else if (HasReachedInterval)
                            {
                                roguePed.Ped.Task.RunTo(roguePed.Victim.Position, false);
                                HasReachedInterval = false;
                            }
                        }
                        else if (roguePed.State == RogueState.SHOOTING_AT_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (roguePed.HasValidVictim())
                            {
                                if (roguePed.DistanceFromVictim() <= MinVictimPedDistanceToShoot)
                                {
                                    if (HasReachedInterval)
                                    {
                                        if (!roguePed.Victim.IsInVehicle())
                                        {
                                            if (!roguePed.Ped.IsInCombatAgainst(roguePed.Victim))
                                            {
                                                PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.FightAgainst(roguePed.Victim); });
                                                //roguePed.Ped.Task.FightAgainst(roguePed.Victim); 
                                            }
                                        }
                                        else
                                        {
                                            roguePed.Ped.Task.ShootAt(roguePed.Victim, 5000, FiringPattern.FullAuto);
                                        }

                                        HasReachedInterval = false;
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
                                if (HasReachedInterval)
                                {
                                    //PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.RunTo(Game.Player.Character.Position, false, 6000); });
                                    roguePed.Ped.Task.RunTo(Game.Player.Character.Position, false);
                                    HasReachedInterval = false;
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                    }
                }

                UI.ShowSubtitle(sub);
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
                    Util.Notify("VRoguePed Error:\n 'roguePed' not found.");
                    return;
                }

                Ped victimPed = null;

                if (targetedVictimPed && Game.Player.IsAlive && Game.Player.IsAiming)
                {
                    Entity targetEntity = null;
                    Util.GetEntityPlayerIsAimingAt(ref targetEntity);

                    if (targetEntity != null && targetEntity.Exists() && Util.IsNPCPed(targetEntity)
                        && !Core.ProcessedPeds.Contains((Ped)targetEntity))
                    {
                        victimPed = (Ped)targetEntity;
                    }

                    UI.Notify("MakePedGoRogue(true): victimPed = " + victimPed != null ? victimPed.ToString() : "NULL");
                }
                else
                {
                    victimPed = PedUtil.GetNearestValidVictimPeds(roguePed, 1, MaxVictimPedSearchDistance, Core.ProcessedPeds).FirstOrDefault();
                }

                if (victimPed == roguePed)
                {
                    Util.Notify("VRoguePed Error:\n 'victimPed' and 'roguePed' are the same Ped.");
                    return;
                }

                //roguePed.IsPersistent = true;
                roguePed.Weapons.Give(RoguePedWeaponHash, 9999, true, true);
                roguePed.MaxHealth = RoguePedHealth;
                roguePed.Health = RoguePedHealth;
                roguePed.CanRagdoll = false;
                roguePed.CanSufferCriticalHits = false;
                roguePed.CanWrithe = false;
                roguePed.MaxSpeed = 100f;
                roguePed.WetnessHeight = 100f;
                roguePed.AlwaysKeepTask = true;
                roguePed.BlockPermanentEvents = true;
                roguePed.FiringPattern = FiringPattern.FullAuto;
                roguePed.CanSwitchWeapons = false;

                PedUtil.PreventPedFromFleeing(roguePed);
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
                    Blip blip = PedUtil.AttachBlipToPed(roguePed, BlipColor.BlueLight, (Core.RoguePeds.Count+1));
                    Core.RoguePeds.Add(new RoguePed(roguePed, victimPed, playerVehicleSeat, blip));
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
                    Core.ProcessedPeds.Add(victimPed);
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
                RoguePedHealth = settings.GetValue<int>("PED_PARAMETERS", "RoguePedHealth", 400);
                String weaponName = settings.GetValue("PED_PARAMETERS", "RoguePedWeaponName", "Pistol50");

                if (weaponName == null || weaponName.Length == 0 ||
                    weaponName.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                {
                    weaponName = "Unarmed";
                }

                RoguePedWeaponHash = (WeaponHash)Enum.Parse(typeof(WeaponHash), weaponName);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ReadPedParamsFromConfig() Error:\n " + e.ToString());
                RoguePedWeaponHash = WeaponHash.Pistol50;
            }
        }
    }
}
