using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRoguePed
{
    internal class TestModule
    {
        public static void MakePedPerformActionProc()
        {
            Ped nearestPed = PedUtil.GetNearestValidRoguePeds(1, 40f, null).FirstOrDefault();
            RaycastResult raycastResult = World.GetCrosshairCoordinates();

            if (nearestPed != null && raycastResult.DitHitAnything)
            {
                //nearestPed.Task.RunTo(raycastResult.HitCoords, false);

                TaskSequence taskSequence = new TaskSequence();
                taskSequence.AddTask.RunTo(raycastResult.HitCoords, false);
                taskSequence.Close();
                nearestPed.Task.PerformSequence(taskSequence);
                taskSequence.Dispose();
            }
        }
    }
}
