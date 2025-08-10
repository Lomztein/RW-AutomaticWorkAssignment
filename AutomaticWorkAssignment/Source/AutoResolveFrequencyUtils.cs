using Lomzie.AutomaticWorkAssignment.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class AutoResolveFrequencyUtils
    {
        public static IEnumerable<AutoResolveFrequencyDef> Defs => DefDatabase<AutoResolveFrequencyDef>.AllDefs;
        public static AutoResolveFrequencyDef None => Defs.First(x => x.resolveCalender == AutoResolveFrequencyDef.ResolveCalender.None);
        public static AutoResolveFrequencyDef Daily => Defs.First(x => x.defName == "AutoResolveFrequency_Daily");

        public static bool ShouldResolveNow(AutoResolveFrequencyDef def, int lastResolveTick, Map map)
        {
            switch (def.resolveCalender)
            {
                case AutoResolveFrequencyDef.ResolveCalender.Year: return ShouldResolveNowYear(def, lastResolveTick, map);
                case AutoResolveFrequencyDef.ResolveCalender.Quadrum: return ShouldResolveNowMonth(def, lastResolveTick, map);
                case AutoResolveFrequencyDef.ResolveCalender.Day: return ShouldResolveNowDay(def, lastResolveTick, map);
                case AutoResolveFrequencyDef.ResolveCalender.Hour: return ShouldResolveNowHour(def, lastResolveTick, map);
                case AutoResolveFrequencyDef.ResolveCalender.None: return false;
                default: return false;
            }
        }

        private static bool ShouldResolveNowHour(AutoResolveFrequencyDef def, int lastResolveTick, Map map)
        {
            int lastHour = GenDate.HourOfDay(lastResolveTick, GetLongitude(map));
            int lastDay = GenDate.DayOfYear(lastResolveTick, GetLongitude(map));
            int currentDay = GenDate.DayOfYear(GenTicks.TicksAbs, GetLongitude(map));
            int nextHour = GetNextValue(lastHour, 24, def.timeBetweenResolve);
            return GenLocalDate.HourInteger(map) >= nextHour || lastDay != currentDay;
        }

        private static bool ShouldResolveNowDay(AutoResolveFrequencyDef def, int lastResolveTick, Map map)
        {
            int lastDay = GenDate.DayOfQuadrum(lastResolveTick, GetLongitude(map));
            Quadrum lastMonth = GenDate.Quadrum(lastResolveTick, GetLongitude(map));
            Quadrum currentMonth = GenDate.Quadrum(GenTicks.TicksAbs, GetLongitude(map));
            int nextDay = GetNextValue(lastDay, 15, def.timeBetweenResolve);
            return GenLocalDate.DayOfQuadrum(map) >= nextDay || lastMonth != currentMonth;
        }

        private static bool ShouldResolveNowMonth(AutoResolveFrequencyDef def, int lastResolveTick, Map map)
        {
            int lastMonth = (int)GenDate.Quadrum(lastResolveTick, GetLongitude(map));
            int lastYear = GenDate.Year(lastResolveTick, GetLongitude(map));
            int currentYear = GenDate.Year(GenTicks.TicksAbs, GetLongitude(map));
            int nextMonth = GetNextValue(lastMonth, 4, def.timeBetweenResolve);
            return (int)GenDate.Quadrum(GenTicks.TicksAbs, GetLongitude(map)) >= nextMonth || lastYear != currentYear;
        }

        private static int GetNextValue(int lastValue, int maxSteps, int stepper)
        {
            int steps = maxSteps / stepper;
            int lastStep = (int)Math.Round(Remap(lastValue, 0, 0, maxSteps, steps));
            int nextStep = lastStep + 1;
            int nextValue = (int)Math.Round(Remap(nextStep, 0, 0, steps, maxSteps));
            return nextValue;
        }

        private static bool ShouldResolveNowYear(AutoResolveFrequencyDef def, int lastResolveTick, Map map)
        {
            if (def.timeBetweenResolve != 1)
                throw new InvalidOperationException("Time steps between resolve for year not supported, 'timeBetweenResolve' must be 1.");

            int lastYear = GenDate.Year(lastResolveTick, GetLongitude(map));
            int currentYear = GenDate.Year(GenTicks.TicksAbs, GetLongitude(map));
            return lastYear != currentYear;
        }

        private static float Remap(float value, float fromMin, float toMin, float fromMax, float toMax)
            => Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));

        private static float GetLongitude(Map map)
        {
            if (map.Parent != null && map.Parent.Tile != null)
                return Find.WorldGrid.LongLatOf(map.Parent.Tile).x;
            return 0f;
        }
    }
}
