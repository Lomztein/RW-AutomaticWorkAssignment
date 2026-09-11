using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using UsefulMarks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.Patches.UsefulMarks
{
    internal static class UsefulMarksCompatibility
    {
        private static readonly FieldInfo AssignedMarkersField = typeof(PawnLabelCustomColors_WorldComponent).GetField("UserAssignedMarkers", BindingFlags.Instance | BindingFlags.NonPublic);

        public static IEnumerable<int> GetMarkerIndices()
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || world.UserMarkers == null)
                yield break;

            for (int i = 0; i < world.UserMarkers.Count; i++)
                yield return i;
        }

        public static string GetMarkerLabel(int markerIndex)
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || world.UserMarkers == null || markerIndex < 0 || markerIndex >= world.UserMarkers.Count)
                return "AWA.UsefulMarks.SelectMark".Translate();

            var marker = world.UserMarkers[markerIndex];
            if (marker == null)
                return "AWA.UsefulMarks.SelectMark".Translate();

            if (!string.IsNullOrWhiteSpace(marker.description))
                return marker.description;

            if (!string.IsNullOrWhiteSpace(marker.iconName))
                return marker.iconName;

            return "AWA.UsefulMarks.MarkLabel".Translate(markerIndex + 1);
        }

        public static Texture2D GetMarkerIcon (int markerIndex)
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || world.UserMarkers == null || markerIndex < 0 || markerIndex >= world.UserMarkers.Count)
                return null;
            var marker = world.UserMarkers[markerIndex];
            if (marker == null)
                return null;
            return marker.icon;
        }

        public static void AddMark(Pawn pawn, int markerIndex)
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || pawn == null || !IsValidMarkerIndex(world, markerIndex) || IsAssigned(pawn, markerIndex))
                return;

            world.ToggleAssign(pawn, markerIndex);
            world.ClearCache(pawn);
        }

        public static void RemoveMark(Pawn pawn, int markerIndex)
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || pawn == null || !IsValidMarkerIndex(world, markerIndex) || !IsAssigned(pawn, markerIndex))
                return;

            world.ToggleAssign(pawn, markerIndex);
            world.ClearCache(pawn);
        }

        public static void ClearMarks(Pawn pawn)
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || pawn == null)
                return;

            foreach (int markerIndex in GetAssignedMarkerIndices(pawn))
                world.ToggleAssign(pawn, markerIndex);

            world.ClearCache(pawn);
        }

        public static bool HasMark(Pawn pawn, int markerIndex)
        {
            if (pawn == null)
                return false;

            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || !IsValidMarkerIndex(world, markerIndex))
                return false;

            return IsAssigned(pawn, markerIndex);
        }

        private static bool IsAssigned(Pawn pawn, int markerIndex)
        {
            return GetAssignedMarkerIndices(pawn).Contains(markerIndex);
        }

        private static IEnumerable<int> GetAssignedMarkerIndices(Pawn pawn)
        {
            var world = PawnLabelCustomColors_WorldComponent.instance;
            if (world == null || pawn == null || AssignedMarkersField == null)
                return Enumerable.Empty<int>();

            var assignments = AssignedMarkersField.GetValue(world) as Dictionary<Pawn, HashSet<int>>;
            if (assignments == null || !assignments.TryGetValue(pawn, out HashSet<int> markerIndices) || markerIndices == null)
                return Enumerable.Empty<int>();

            return markerIndices.ToList();
        }

        private static bool IsValidMarkerIndex(PawnLabelCustomColors_WorldComponent world, int markerIndex)
        {
            return markerIndex >= 0 && world.UserMarkers != null && markerIndex < world.UserMarkers.Count;
        }
    }
}
