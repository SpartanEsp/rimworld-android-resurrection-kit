﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AndroidResurrectionKit
{
    // Token: 0x0200007F RID: 127
    public class JobDriver_ResurrectAndroid : JobDriver
    {
        // Token: 0x170000B3 RID: 179
        // (get) Token: 0x06000363 RID: 867 RVA: 0x00022338 File Offset: 0x00020738
        internal Corpse Corpse
        {
            get
            {
                return (Corpse)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        // Token: 0x170000B4 RID: 180
        // (get) Token: 0x06000364 RID: 868 RVA: 0x00022360 File Offset: 0x00020760
        private Thing Item
        {
            get
            {
                return this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        // Token: 0x06000365 RID: 869 RVA: 0x00022384 File Offset: 0x00020784
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Corpse;
            Job job = this.job;
            bool result;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.Item;
                job = this.job;
                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            else
            {
                result = false;
            }
            return result;
        }

        // Token: 0x06000366 RID: 870 RVA: 0x000223EC File Offset: 0x000207EC
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
            Toil prepare = Toils_General.Wait(1200, TargetIndex.None);
            prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedOrNull(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return prepare;
            yield return Toils_General.Do(new Action(this.Resurrect));
            yield break;
        }

        // Token: 0x06000367 RID: 871 RVA: 0x00022410 File Offset: 0x00020810
        private void Resurrect()
        {
            Pawn innerPawn = this.Corpse.InnerPawn;

            bool canResurrect = false;
            string deadPawnRaceName = innerPawn.kindDef.race.defName.ToLower();
            switch (Item.def.defName)
            {
                case "RepairKitResurrectorB":
                    canResurrect = deadPawnRaceName.Contains("1tier");
                    break;
                case "RepairKitResurrectorA":
                    canResurrect = deadPawnRaceName.Contains("1tier") || deadPawnRaceName.Contains("2tier");
                    break;
                case "RepairKitResurrectorS":
                    canResurrect = deadPawnRaceName.Contains("android");
                    break;
            }

            if (canResurrect)
            {
                ResurrectionUtility.Resurrect(innerPawn);
                Messages.Message("MessagePawnResurrected".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent, true);
                this.Item.SplitOff(1).Destroy(DestroyMode.Vanish);
            }
            else
                Messages.Message("CantRepair".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.RejectInput, true);
        }

        // Token: 0x04000238 RID: 568
        private const TargetIndex CorpseInd = TargetIndex.A;

        // Token: 0x04000239 RID: 569
        private const TargetIndex ItemInd = TargetIndex.B;

        // Token: 0x0400023A RID: 570
        private const int DurationTicks = 600;
    }
}
