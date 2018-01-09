using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using UnityEngine;

namespace M_No_Death_Randomness
{
    public static class Main
    {
        public static readonly FieldInfo _pawn = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo _MakeDowned = typeof(Pawn_HealthTracker).GetMethod("MakeDowned", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo _MakeUndowned = typeof(Pawn_HealthTracker).GetMethod("MakeUndowned", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void CheckForStateChange(this Pawn_HealthTracker _this, DamageInfo? _dinfo, Hediff _hediff)
        {
            Pawn pawn = (Pawn)_pawn.GetValue(_this);

            if (!_this.Dead)
            {
                if (_this.ShouldBeDead())
                {
                    if (!pawn.Destroyed)
                    {
                        bool flag = PawnUtility.ShouldSendNotificationAbout(pawn);
                        Caravan caravan = pawn.GetCaravan();
                        pawn.Kill(_dinfo);
                        if (flag)
                        {
                            _this.NotifyPlayerOfKilled(_dinfo, _hediff, caravan);
                        }
                    }
                    return;
                }
                if (!_this.Downed)
                {
                    if (_this.ShouldBeDowned())
                    {
                        float num = (!pawn.RaceProps.Animal) ? 0.67f : 0.47f;
                        if (!_this.forceIncap && _dinfo.HasValue && _dinfo.Value.Def.externalViolence && (pawn.Faction == null || !pawn.Faction.IsPlayer) && !pawn.IsPrisonerOfColony && pawn.RaceProps.IsFlesh && Rand.Value < num)
                        {
                            //pawn.Kill(_dinfo);
                            _MakeDowned.Invoke(_this, new object[] { _dinfo, _hediff });
                            Log.Warning(pawn.Name + " would have been killed by random.");
                            return;
                        }
                        _this.forceIncap = false;
                        _MakeDowned.Invoke(_this, new object[] { _dinfo, _hediff });
                        Log.Warning(pawn.Name + " would have survived the random.");
                        return;
                    }
                    else if (!_this.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    {
                        if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.jobs != null && pawn.CurJob != null)
                        {
                            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                        }
                        if (pawn.equipment != null && pawn.equipment.Primary != null)
                        {
                            if (pawn.InContainerEnclosed)
                            {
                                pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.holdingOwner);
                            }
                            else if (pawn.SpawnedOrAnyParentSpawned)
                            {
                                ThingWithComps thingWithComps;
                                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out thingWithComps, pawn.PositionHeld, true);
                            }
                            else
                            {
                                pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
                            }
                        }
                    }
                }
                else if (!_this.ShouldBeDowned())
                {
                    _MakeUndowned.Invoke(_this, null);
                    return;
                }
            }
        }

        internal static bool ShouldBeDead(this Pawn_HealthTracker _this)
        {
            Pawn pawn = (Pawn)_pawn.GetValue(_this);

            if (_this.Dead)
            {
                return true;
            }
            for (int i = 0; i < _this.hediffSet.hediffs.Count; i++)
            {
                if (_this.hediffSet.hediffs[i].CauseDeathNow())
                {
                    return true;
                }
            }
            List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
            for (int j = 0; j < allDefsListForReading.Count; j++)
            {
                PawnCapacityDef pawnCapacityDef = allDefsListForReading[j];
                bool flag = (!pawn.RaceProps.IsFlesh) ? pawnCapacityDef.lethalMechanoids : pawnCapacityDef.lethalFlesh;
                if (flag && !_this.capacities.CapableOf(pawnCapacityDef))
                {
                    return true;
                }
            }
            float num = PawnCapacityUtility.CalculatePartEfficiency(_this.hediffSet, pawn.RaceProps.body.corePart, false, null);
            return num <= 0.0001f;
        }

        internal static bool ShouldBeDowned(this Pawn_HealthTracker _this)
        {
            return _this.InPainShock || !_this.capacities.CanBeAwake || !_this.capacities.CapableOf(PawnCapacityDefOf.Moving);
        }
    }
}