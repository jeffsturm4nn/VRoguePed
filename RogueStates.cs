﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;
using GTA.Native;

using static VRoguePed.PedModule;

namespace VRoguePed
{
    internal class RogueStates
    {
        /*public static void PerformRunningTowardsVictim(RoguePed roguePed, bool hasReachedInterval)
        {
            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
            {
                roguePed.State = RoguePedState.RUNNING_TOWARDS_PLAYER;
            }
            else if (RoguePedsFollowPlayer)
            {
                roguePed.State = RoguePedState.FOLLOWING_PLAYER;
            }
            else if (roguePed.HasValidVictim())
            {
                if (roguePed.DistanceFromVictim() <= MaxVictimPedOnFootChaseDistance)
                {
                    roguePed.State = RoguePedState.ATTACKING_VICTIM;
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
                roguePed.State = RoguePedState.LOOKING_FOR_VICTIM;
            }
        }*/

        public static void PerformLookinForVictim(RoguePed roguePed, bool hasReachedInterval)
        {
            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
            {
                roguePed.State = RoguePedState.RUNNING_TOWARDS_PLAYER;
            }
            else if (RoguePedsFollowPlayer)
            {
                roguePed.State = RoguePedState.FOLLOWING_PLAYER;
            }
            else if (RoguePedsEscortPlayer)
            {
                roguePed.State = RoguePedState.ESCORT_PLAYER;
            }
            else if (hasReachedInterval)
            {
                if (!roguePed.HasValidVictim())
                {
                    if (!PedUtil.IsPedWanderingAround(roguePed.Ped) || !roguePed.Ped.IsWalking)
                    {
                        roguePed.Ped.Task.ClearAll();
                        PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.WanderAround(Game.Player.Character.Position, MaxRoguePedWanderDistance); });
                        //roguePed.Ped.Task.WanderAround(Game.Player.Character.Position, MaxRoguePedWanderDistance);
                    }
                }
                else
                {
                    //roguePed.Ped.Task.ClearAll();
                    roguePed.State = RoguePedState.ATTACKING_VICTIM;
                }

                roguePed.Ped.WetnessHeight = 6f;
            }
        }

        public static void PerformAttackingVictim(RoguePed roguePed, bool hasReachedInterval)
        {
            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
            {
                roguePed.State = RoguePedState.RUNNING_TOWARDS_PLAYER;
            }
            else if (RoguePedsFollowPlayer)
            {
                roguePed.State = RoguePedState.FOLLOWING_PLAYER;
            }
            else if (roguePed.HasValidVictim())
            {
                if (hasReachedInterval)
                {
                    //if (!roguePed.Victim.Ped.IsInVehicle())
                    //{
                    //    if (!roguePed.IsInCombatWithVictim())
                    //    {
                    //        //roguePed.Ped.Task.ClearAll();
                    //        roguePed.Ped.Task.FightAgainst(roguePed.Victim.Ped, -1);
                    //        //PedUtil.PerformTaskSequence(roguePed.Ped,
                    //        //        ts => { ts.AddTask.FightAgainst(roguePed.Victim.Ped, -1); });
                    //    }
                    //}
                    //else
                    {
                        //if (!roguePed.Ped.IsInFlyingVehicle && roguePed.DistanceFromVictim() > 10f)
                        //{
                        //    roguePed.Ped.Task.ShootAt(roguePed.Victim.Ped, 4000, FiringPattern.FullAuto);
                        //}
                        //else
                        {
                            if (!roguePed.IsInCombatWithVictim())
                            {
                                roguePed.Ped.Task.ClearAll();
                                //roguePed.Ped.Task.FightAgainst(roguePed.Victim.Ped, -1);
                                PedUtil.PerformTaskSequence(roguePed.Ped,
                                    ts => { ts.AddTask.FightAgainst(roguePed.Victim.Ped, -1); });
                            }
                        }
                    }
                }
            }
            else
            {
                if (RoguePedsEscortPlayer)
                {
                    roguePed.State = RoguePedState.ESCORT_PLAYER;
                }
                else
                {
                    roguePed.State = RoguePedState.LOOKING_FOR_VICTIM;
                }
            }
        }

        public static void PerformRunningTowardsPlayer(RoguePed roguePed, bool hasReachedInterval)
        {
            if (RoguePedsFollowPlayer)
            {
                roguePed.State = RoguePedState.FOLLOWING_PLAYER;
            }
            else if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
            {
                if (hasReachedInterval)
                {
                    roguePed.Ped.Task.RunTo(Game.Player.Character.Position, false);
                }
            }
            else
            {
                roguePed.State = RoguePedState.LOOKING_FOR_VICTIM;
            }
        }

        public static void PerformFollowingPlayer(RoguePed roguePed, bool hasReachedInterval)
        {
            if (RoguePedsFollowPlayer)
            {
                MakeRoguePedFollowPlayer(roguePed, hasReachedInterval);
            }
            else
            {
                if (roguePed.Ped.IsInVehicle())
                {
                    PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.LeaveVehicle(); });
                }
                else
                {
                    roguePed.PlayerVehicleSeat = VehicleSeat.None;
                    roguePed.State = RoguePedState.LOOKING_FOR_VICTIM;
                }
            }
        }

        public static void PerformEscortingPlayer(RoguePed roguePed, bool hasReachedInterval)
        {
            if (RoguePedsEscortPlayer)
            {
                if (!roguePed.HasValidVictim())
                {
                    MakeRoguePedFollowPlayer(roguePed, hasReachedInterval);
                }
                else
                {
                    roguePed.State = RoguePedState.ATTACKING_VICTIM;
                } 
            }
            else
            {
                if (roguePed.Ped.IsInVehicle())
                {
                    PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.LeaveVehicle(); });
                }
                else
                {
                    roguePed.PlayerVehicleSeat = VehicleSeat.None;
                    roguePed.State = RoguePedState.LOOKING_FOR_VICTIM;
                }
            }
        }

        private static void MakeRoguePedFollowPlayer(RoguePed roguePed, bool hasReachedInterval)
        {
            if (hasReachedInterval)
            {
                Vector3 offsetFromPlayer = new Vector3(0f, 2.5f, 0f);

                if (Game.Player.Character.IsRunning || Game.Player.Character.IsSprinting || roguePed.DistanceFromPlayer() > 12f)
                {
                    roguePed.Ped.Task.RunTo((Game.Player.Character.Position + offsetFromPlayer), false, -1);
                }
                else if (!Game.Player.Character.IsInVehicle())
                {
                    roguePed.PlayerVehicleSeat = VehicleSeat.None;

                    if (!roguePed.Ped.IsInVehicle())
                    {
                        if (roguePed.DistanceFromPlayer() > 4.0f)
                        {
                            roguePed.Ped.Task.GoTo(Game.Player.Character, offsetFromPlayer, -1);
                        }
                    }
                    else
                    {
                        PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.LeaveVehicle(); });
                    }
                }
                else if (!roguePed.Ped.IsInVehicle(Game.Player.Character.CurrentVehicle) && !roguePed.Ped.IsGettingIntoAVehicle)
                {
                    if (roguePed.PlayerVehicleSeat == VehicleSeat.None)
                    {
                        roguePed.PlayerVehicleSeat = VehicleUtil.GetPlayerVehicleFreeSeat();
                    }

                    if (roguePed.PlayerVehicleSeat != VehicleSeat.None)
                    {
                        roguePed.Ped.Task.ClearAll();

                        PedUtil.PerformTaskSequence(roguePed.Ped, ts =>
                        {
                            ts.AddTask.EnterVehicle(Game.Player.Character.CurrentVehicle, roguePed.PlayerVehicleSeat, -1);
                        });
                    }
                }
            }
        }
    }
}

