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
            {
                return;
            }
            
            var energyPerSecond = CompTempControl.Props.energyPerSecond;

            if (tempDifference < 0) // In this scenario, the ambient temperature is lower than the target temperature.
            {
                energyPerSecond = Math.Abs(energyPerSecond);
            }

            var tempChange = GenTemperature.ControlTemperatureTempChange(Position, Map, GetEnergyLimit(energyPerSecond), targetTemp);
            
            var atHighPower = !Mathf.Approximately(tempChange, 0.0f);
            var powerTraderProps = CompPowerTrader.Props;

            if (atHighPower)
            {
                this.GetRoomGroup().Temperature += tempChange;
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