using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;

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
                        //&& v.WheelType != VehicleWheelType.BikeWheels
                        && v.IsStopped
                        )
                    .OrderBy(p => Math.Abs(p.Position.DistanceTo(ped.Position)))
                    .FirstOrDefault();

            return vehicle;
        }

        public static VehicleSeat GetPlayerVehicleFreeSeat()
        {
            Ped player = Game.Player.Character;

            if(player.IsInVehicle() && player.CurrentVehicle.PassengerSeats > 0)
            {
                for(int i = 0; i<player.CurrentVehicle.PassengerSeats; i++)
                {
                    VehicleSeat vehicleSeat = (VehicleSeat)i;

                    if (vehicleSeat != VehicleSeat.Driver && 
                        !Entity.Exists(player.CurrentVehicle.GetPedOnSeat(vehicleSeat)) && 
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
            for (int i = 0; i < vehicle.PassengerSeats; i++)
            {
                VehicleSeat vehicleSeat = (VehicleSeat)i;

                if (vehicle.GetPedOnSeat(vehicleSeat) == ped)
                {
                    return vehicleSeat;
                }
            }

            return VehicleSeat.None;
        }
    }
}
