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
        public static Vehicle GetNearesVehicle(Ped ped)
        {
            Vehicle vehicle = World.GetNearbyVehicles(ped, 40f)
                     .Where(v => v != null
                        && v.Exists()
                        && v.IsDriveable
                        && !v.IsUpsideDown
                        //&& v.WheelType != VehicleWheelType.BikeWheels
                        && v.IsStopped)
                    .OrderBy(p => Math.Abs(p.Position.DistanceTo(ped.Position)))
                    .FirstOrDefault();

            return vehicle;
        }
    }
}
