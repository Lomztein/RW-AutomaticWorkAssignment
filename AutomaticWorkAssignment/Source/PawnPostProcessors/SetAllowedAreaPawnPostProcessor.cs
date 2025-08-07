using AutomaticWorkAssignment;
using RimWorld;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetAllowedAreaPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public Area AllowedArea;
        private string _allowedAreaName;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref AllowedArea, "area");

            if (Scribe.mode == LoadSaveMode.Saving && AllowedArea != null)
                _allowedAreaName = AllowedArea.Label;

            Scribe_Values.Look(ref _allowedAreaName, "allowedAreaName");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && _allowedAreaName != null && AllowedArea == null)
                Find.Root.StartCoroutine(DelayedSetArea());
        }

        private IEnumerator DelayedSetArea()
        {
            // Continuously retry getting area if gravship landing is in progress.
            do
            {
                AllowedArea = MapWorkManager.LastInitializedMap.areaManager.AllAreas.Find(x => x.Label == _allowedAreaName);
                if (AllowedArea != null) break;
                yield return new WaitForSeconds(1f);
            } while (GravshipUtils.LandingInProgress);
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn.Map?.IsPlayerHome ?? false)
            {
                pawn.playerSettings.AreaRestrictionInPawnCurrentMap = AllowedArea;
            }
        }
    }
}
