using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;
using GTA.Native;

namespace VRoguePed
{
    internal class VictimData
    {
        public VictimData() { }
        public VictimData(Ped ped, float distance, bool isAttackingOtherRoguePeds, bool isCop, bool isAttackingTarget, bool isAttackingPlayer)
        {
            Ped = ped;
            Distance = distance;
            IsAttackingOtherRoguePeds = isAttackingOtherRoguePeds;
            IsCop = isCop;
            IsAttackingTarget = isAttackingTarget;
            IsAttackingPlayer = isAttackingPlayer;
        }

        public Ped Ped { get; set; }
        public float Distance { get; set; }

        public bool IsAttackingOtherRoguePeds {  get; set; }
        public bool IsCop {  get; set; }
        public bool IsAttackingTarget {  get; set; }
        public bool IsAttackingPlayer {  get; set; }
        public RoguePed AttackedRoguePed { get; set; }
        public int AttackersCount { get; set; }
    }
}
