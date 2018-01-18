using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RidiculousCheat
{
    public class TempurMaticThingDef : Building
    {
        public CompTempControl CompTempControl;
        public CompPowerTrader CompPowerTrader;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CompTempControl = GetComp<CompTempControl>();
            CompPowerTrader = GetComp<CompPowerTrader>();
        }

        public override void TickRare()
        {
            if (!CompPowerTrader.PowerOn)
            {
                return;
            }

            var targetTemp = CompTempControl.targetTemperature;

            var tempDifference = AmbientTemperature - targetTemp;

            if (Math.Abs(tempDifference) < 1.0f)
                return;
            
            var energyPerSecond = CompTempControl.Props.energyPerSecond;

            var a = GenTemperature.ControlTemperatureTempChange(Position, Map, GetEnergyLimit(energyPerSecond), targetTemp);

            var atHighPower = !Mathf.Approximately(a, 0.0f);
            var powerTraderProps = CompPowerTrader.Props;

            if (atHighPower)
            {
                this.GetRoomGroup().Temperature += a;
                CompPowerTrader.PowerOutput = -powerTraderProps.basePowerConsumption;
            }
            else
            {
                CompPowerTrader.PowerOutput = -powerTraderProps.basePowerConsumption * CompTempControl.Props.lowPowerConsumptionFactor;
            }

            CompTempControl.operatingAtHighPower = atHighPower;
        }

        private static float GetEnergyLimit(float energyPerSecond)
        {
            return (float)(energyPerSecond * 4.16666650772095);
        }
    }
}