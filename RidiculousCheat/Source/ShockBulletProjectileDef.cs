using System;
using RimWorld;
using Verse;

namespace RidiculousCheat
{
    public class ShockBulletProjectileDef : Bullet
    {
        public ShockBulletThingDef Def => def as ShockBulletThingDef;
        
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def == null || hitThing == null || !(hitThing is Pawn hitPawn) || hitPawn.health == null) return;

            try
            {
                var knockoutOnPawn = hitPawn.health.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);

                if (knockoutOnPawn != null) return; //It's been knocked out once. No need to do it again.

                var hediff = HediffMaker.MakeHediff(Def.HediffToAdd, hitPawn);
                
                hitPawn.health.AddHediff(hediff);

                Messages.Message($"{hitPawn.Label} was knocked out!", MessageTypeDefOf.NeutralEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}