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

        private static volatile bool hasReachedInterval = false;
        private static int timeCounter = 0;

        public static int FriendlyRoguePedsGroupHash = -1;
        public static int RoguePedTargetsGroupHash = -2;
        public static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;
        public static int RoguePedHealth = 500;
        public static int RoguePedClearTasksIntervalInSecs = 300;
        public static int BatchRoguePedCount = 5;
        public static int MaxRoguePedsPerTarget = 2;
        public static float MaxRoguePedRecruitDistance = 40f;
        public static float MaxVictimPedOnFootChaseDistance = 40f;
        public static float MaxVictimPedInVehicleChaseDistance = 40f;
        public static float MaxRoguePedDistanceFromPlayer = 60f;
        //public static float MinVictimPedDistanceToShoot = 19f;
        public static float MaxRoguePedWanderDistance = 30f;
        public static float MaxRoguePedDistanceBeforeDisband = 80f;
        public static bool RoguePedsFollowPlayer = true;
        public static bool RoguePedsBodyguardMode = false;

        public static string sub = "";

        public static void InitPedRelationshipGroups()
        {
            FriendlyRoguePedsGroupHash = World.AddRelationshipGroup("FriendlyRoguePeds");
            World.SetRelationshipBetweenGroups(Relationship.Companion, FriendlyRoguePedsGroupHash, Game.Player.Character.RelationshipGroup);
        }

        public static void ToggleRoguePedsFollowPlayerProc()
        {
            RoguePedsFollowPlayer = !RoguePedsFollowPlayer;
        }

        public static void KillAllRoguePedsProc()
        {
            for (int i = 0; i < Core.RoguePeds.Count; i++)
            {
                RoguePed roguePed = Core.RoguePeds[i];

                if (roguePed != null && roguePed.IsValid())
                {
                    roguePed.IsInUse = false;
                    roguePed.Ped.Kill();
                }
            }
        }

        public static void MakeBatchPedsGoRogueProc()
        {
            for (int i = 0; i < BatchRoguePedCount; i++)
            {
                MakePedGoRogue(false, false);
            }
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
                    bool removeVictimPed = false;
                    bool clearTasks = false;

                    if (roguePed != null)
                    {
                        if (!roguePed.IsValid() || roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceBeforeDisband)
                        {
                            PedUtil.RemoveRoguePed(roguePed);
                        }
                        else if (RoguePedsFollowPlayer)
                        {
                            roguePed.State = RoguePedState.FOLLOWING_PLAYER;
                        }
                        else if (roguePed.State != RoguePedState.RUNNING_TOWARDS_PLAYER)
                        {
                            if (roguePed.HasValidVictim())
                            {
                                if (!PedUtil.IsVictimInAttackingRange(roguePed, roguePed.Victim))
                                {
                                    removeVictimPed = true;
                                    roguePed.State = RoguePedState.LOOKING_FOR_VICTIM;
                                }
                            }
                            else if (roguePed.Victim != null)
                            {
                                clearTasks = true;
                                removeVictimPed = true;
                            }

                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RoguePedState.RUNNING_TOWARDS_PLAYER;
                            }
                        }

                        if (removeVictimPed)
                        {
                            PedUtil.RemoveVictim(roguePed.Victim);
                            roguePed.Victim = null;
                        }

                        if (clearTasks)
                        {
                            roguePed.Ped.Task.ClearAll();
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
                const int timeInterval = 200 / Constants.UPDATE_INTERVAL;
                bool hasReachedInterval = false;

                if (++timeCounter >= timeInterval)
                {
                    timeCounter = 0;
                    hasReachedInterval = true;
                }

                sub += "RogPeds = " + Core.RoguePeds.Count;
                sub += " | ProcPeds = " + Core.ProcessedPeds.Count;
                sub += " | ProcCountMap = " + Core.ProcessedPedCountMap.Count;
                sub += "\n";

                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null && roguePed.IsValid())
                    {
                        //sub += "[i=" + i + "| TS=" + roguePed.Ped.TaskSequenceProgress +
                        //    "| W=" + (PedUtil.IsPedWanderingAround(roguePed.Ped) ? "1" : "0") +
                        //    "| D=" + ((int)roguePed.DistanceFromPlayer()) +
                        //    "| S=" + roguePed.State.ToString().Split('_')[0].Substring(0, 2) +
                        //    "| V=" + (!Util.IsValid(roguePed.Victim) ? "-1" : ("@" + ((int)roguePed.DistanceFromVictim()) + "]"));

                        bool hasToClearTasks = false;

                        if (roguePed.ClearTasksTime > 0)
                        {
                            roguePed.ClearTasksTime -= Constants.UPDATE_INTERVAL;
                        }
                        else
                        {
                            hasToClearTasks = true;
                            roguePed.ClearTasksTime = (RoguePedClearTasksIntervalInSecs * 1000);
                        }

                        if (hasToClearTasks)
                        {
                            if (!roguePed.Ped.IsInVehicle())
                            {
                                roguePed.Ped.Task.ClearAll();
                            }
                        }
                        else if (roguePed.State == RoguePedState.LOOKING_FOR_VICTIM)
                        {
                            RogueStates.PerformLookinForVictim(roguePed, hasReachedInterval);
                        }
                        else if (roguePed.State == RoguePedState.ATTACKING_VICTIM)
                        {
                            RogueStates.PerformAttackingVictim(roguePed, hasReachedInterval);
                        }
                        else if (roguePed.State == RoguePedState.RUNNING_TOWARDS_PLAYER)
                        {
                            RogueStates.PerformRunningTowardsPlayer(roguePed, hasReachedInterval);
                        }
                        else if (roguePed.State == RoguePedState.FOLLOWING_PLAYER)
                        {
                            RogueStates.PerformFollowingPlayer(roguePed, hasReachedInterval);
                        }
                    }
                }

                Util.Subtitle(sub);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed UpdateRoguePedsState() Error:\n " + e.ToString());
            }
        }

        public static void UpdateRoguePedTargets()
        {
            List<RoguePed> idleRoguePeds = Core.RoguePeds.Where(rp => rp.State == RoguePedState.LOOKING_FOR_VICTIM).ToList();

            if (idleRoguePeds.Count > 0)
            {
                List<VictimData> victimDataList = PedUtil.GetCurrentValidVictimPedsData();
                List<VictimData> assignedVictimPedList = new List<VictimData>();
                List<VictimData> victimDataListRef = null;

                for (int victimsPerTarget = 1; (idleRoguePeds.Count > 0 && victimsPerTarget <= MaxRoguePedsPerTarget); victimsPerTarget++)
                {
                    if(victimsPerTarget > 1)
                    {
                        victimDataListRef = assignedVictimPedList;
                    }
                    else
                    {
                        victimDataListRef = victimDataList;
                    }

                    for (int i = 0; i < idleRoguePeds.Count; i++)
                    {
                        RoguePed roguePed = idleRoguePeds[i];
                        VictimData nearestVictimData = PedUtil.GetNearestVictimPed(roguePed, victimDataListRef);

                        if (nearestVictimData != null)
                        {
                            if (!roguePed.HasValidVictim())
                            {
                                roguePed.Victim = new VictimPed(nearestVictimData);
                                PedUtil.InsertVictimPed(roguePed.Victim);
                                
                                idleRoguePeds.Remove(roguePed);
                                assignedVictimPedList.Add(nearestVictimData);
                            }
                            else if(nearestVictimData.IsAttackingPlayer)
                            {
                                VictimPed newVictimPed = new VictimPed(nearestVictimData);
                                PedUtil.ResetRoguePedVictim(roguePed, newVictimPed);

                                idleRoguePeds.Remove(roguePed);
                                assignedVictimPedList.Add(nearestVictimData);
                            }
                        }
                    }  
                }
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
                    roguePed = PedUtil.GetNearestValidRoguePeds(1, MaxRoguePedRecruitDistance, Core.ProcessedPeds)
                        .FirstOrDefault();
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

                if (!Util.IsValid(roguePed))
                {
                    return;
                }

                roguePed.Task.ClearAll();

                PedUtil.SetRoguePedParameters(roguePed);
                PedUtil.MakePedCombatResilient(roguePed);

                Blip blip = PedUtil.AttachBlipToPed(roguePed, BlipColor.BlueLight, (Core.RoguePeds.Count + 1));
                RoguePed instance = new RoguePed(roguePed, null, VehicleSeat.None, blip, (RoguePedClearTasksIntervalInSecs * 1000));

                PedUtil.InsertRoguePed(instance);

                AddPedToFriendlyGroup(roguePed);
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
                MaxVictimPedOnFootChaseDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxVictimPedOnFootChaseDistance", 40);
                MaxVictimPedInVehicleChaseDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxVictimPedInVehicleChaseDistance", 40);
                MaxRoguePedDistanceFromPlayer = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedDistanceFromPlayer", 60);
                //MinVictimPedDistanceToShoot = settings.GetValue<int>("PED_PARAMETERS", "MinVictimPedDistanceToShoot", 19);
                MaxRoguePedWanderDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedWanderDistance", 30);
                MaxRoguePedDistanceBeforeDisband = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedDistanceBeforeDisband", 80);
                RoguePedClearTasksIntervalInSecs = settings.GetValue<int>("PED_PARAMETERS", "RoguePedLifetimeInSeconds", 300);
                BatchRoguePedCount = settings.GetValue<int>("PED_PARAMETERS", "BatchRoguePedCount", 5);
                MaxRoguePedsPerTarget = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedsPerTarget", 2);
                RoguePedsBodyguardMode = settings.GetValue<bool>("PED_PARAMETERS", "RoguePedsBodyguardMode", false);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ReadPedParamsFromConfig() Error:\n " + e.ToString());
            }
        }
    }
}
