using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UsefulMarks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    internal class HarmonyPatch
    {
        public void DoPatch ()
        {
            var harmony = new Harmony("Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks");

            MethodInfo createNewMarker = AccessTools.Method(typeof(PawnLabelCustomColors_WorldComponent), "CreateNewMarker");
            harmony.Patch(createNewMarker, postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(IncrementAll)));

            MethodInfo createNewMarkerFromData = AccessTools.Method(typeof(PawnLabelCustomColors_WorldComponent), "CreateNewMarkerFromData");
            harmony.Patch(createNewMarkerFromData, postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(IncrementAll)));

            MethodInfo moveMarker = AccessTools.Method(typeof(PawnLabelCustomColors_WorldComponent), "MoveMarker");
            harmony.Patch(moveMarker, postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(MoveMarkerPostfix)));

            MethodInfo removeMarker = AccessTools.Method(typeof(PawnLabelCustomColors_WorldComponent), "RemoveMarker");
            harmony.Patch(removeMarker, postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(RemoveMarkerPostfix)));

            harmony.PatchAll();
            Log.Message("[AWA] Applied UsefulMarks harmony patches");
        }

        private static void IncrementAll()
        {
            var postProcessors = GetAllMarkPawnPostProcessors();
            foreach (var pp in postProcessors)
            {
                pp.MarkerIndex++;
            }
        }

        private static void MoveMarkerPostfix(int index, int newPosition)
        {
            var postProcessors = GetAllMarkPawnPostProcessors();
            foreach (var pp in postProcessors)
            {
                if (pp.MarkerIndex == index)
                {
                    pp.MarkerIndex = newPosition;
                }
                else if (pp.MarkerIndex == newPosition)
                {
                    pp.MarkerIndex = index;
                }
            }
        }

        private static void RemoveMarkerPostfix(int index)
        {
            var postProcessors = GetAllMarkPawnPostProcessors();
            foreach (var pp in postProcessors)
            {
                if (pp.MarkerIndex == index)
                {
                    pp.MarkerIndex = -1;
                }
                else if (pp.MarkerIndex > index)
                {
                    pp.MarkerIndex--;
                }
            }
        }

        private static IEnumerable<MarkPawnPostProcessorBase> GetAllMarkPawnPostProcessors()
        {
            List<Map> maps = Find.Maps;
            foreach (var map in maps)
            {
                MapWorkManager mapWorkManager = map.GetComponent<MapWorkManager>();
                foreach (WorkSpecification spec in mapWorkManager.WorkList)
                {
                    foreach (var processor in Utils.FindRecursive(spec.PostProcessors, (IPawnSetting setting) => setting is MarkPawnPostProcessorBase).Cast<MarkPawnPostProcessorBase>())
                    {
                        yield return processor;
                    }
                }
            }
        }
    }
}
