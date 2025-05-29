using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;

namespace VRoguePed
{
    internal class VehicleUtil
    {
        public static Vehicle GetNearesVehicle(Ped ped, float maxRadius = 45f)
        {
            Vehicle vehicle = World.GetNearbyVehicles(ped, maxRadius)
                     .Where(v => v != null
                        && v.Exists()
                        && !v.IsOnFire
                        && !v.IsUpsideDown
                        && v.IsDriveable
                        && v.IsStopped
                        )
                    .OrderBy(p => Math.Abs(p.Position.DistanceTo(ped.Position)))
                    .FirstOrDefault();

            return vehicle;
        }

        public static VehicleSeat GetPlayerVehicleFreeSeat()
        {
            Ped player = Game.Player.Character;

            if (player.IsSittingInVehicle() && player.CurrentVehicle.PassengerSeats > 0)
            {
                for (int i = player.CurrentVehicle.PassengerSeats - 1; i >= 0; i--)
                {
                    VehicleSeat vehicleSeat = (VehicleSeat)i;

                    if (vehicleSeat != VehicleSeat.Driver &&
                        (player.CurrentVehicle.IsSeatFree(vehicleSeat) ||
                        player.CurrentVehicle.GetPedOnSeat(vehicleSeat).IsDead) &&
                        Core.RoguePeds.Where(p => p.IsValid() && p.PlayerVehicleSeat == vehicleSeat).Count() == 0)
                    {
                        return vehicleSeat;
                    }
                }
            }

            return VehicleSeat.None;
        }

        public static VehicleSeat GetSeatPedIsSittingOn(Ped ped, Vehicle vehicle)
        {
            if (ped != null && ped.Exists())
            {
                for (int i = 0; i < vehicle.PassengerSeats; i++)
                {
                    VehicleSeat vehicleSeat = (VehicleSeat)i;

                    if (vehicle.GetPedOnSeat(vehicleSeat) == ped)
                    {
                        return vehicleSeat;
                    }
                }
            }

            return VehicleSeat.None;
        }

        public static void RecruitPedAsDriver(Ped ped, Vehicle vehicle, Vector3 destination)
        {
            if (ped != null && ped.Exists() && vehicle != null && vehicle.Exists())
            {
                TaskSequence ts = new TaskSequence();
                ts.AddTask.ClearAllImmediately();

                if (destination != null)
                {
                    ts.AddTask.DriveTo(vehicle, destination, 10f, 90f, (int)DrivingStyle.Rushed);
                }
                else
                {
                    ts.AddTask.EnterVehicle(vehicle, VehicleSeat.Driver);
                }

                ts.AddTask.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                ts.Close();

                ped.Task.PerformSequence(ts);

                ts.Dispose();
            }
        }
    }
}
