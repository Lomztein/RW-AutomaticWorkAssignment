using RimWorld;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class TimePawnFitness : PawnSetting, IPawnFitness
    {
        public enum TimeComponent { Hour, DayOfMonth, MonthOfYear, DayOfYear, Year }

        public TimeComponent Component = TimeComponent.Hour;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            Map map = request.Map;
            switch (Component)
            {
                case TimeComponent.Hour:
                    return GenLocalDate.HourInteger(map);
                case TimeComponent.DayOfMonth:
                    return GenLocalDate.DayOfQuadrum(map);
                case TimeComponent.MonthOfYear:
                    return (int)GenDate.Quadrum(GenTicks.TicksAbs, GetLongitude(map));
                case TimeComponent.DayOfYear:
                    return GenDate.DayOfYear(GenTicks.TicksAbs, GetLongitude(map));
                case TimeComponent.Year:
                    return GenDate.Year(GenTicks.TicksAbs, GetLongitude(map));
                default:
                    return 0f;
            }
        }

        private static float GetLongitude(Map map)
        {
            if (map?.Parent?.Tile != null)
                return Find.WorldGrid.LongLatOf(map.Parent.Tile).x;
            return 0f;
        }

        public static string GetLabel(TimeComponent component)
        {
            switch (component)
            {
                case TimeComponent.Hour: return "AWA.TimeComponent.Hour";
                case TimeComponent.DayOfMonth: return "AWA.TimeComponent.DayOfMonth";
                case TimeComponent.MonthOfYear: return "AWA.TimeComponent.MonthOfYear";
                case TimeComponent.DayOfYear: return "AWA.TimeComponent.DayOfYear";
                case TimeComponent.Year: return "AWA.TimeComponent.Year";
                default:
                    throw new ArgumentException($"Unknown time component {component}");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Component, "component", TimeComponent.Hour);
        }
    }
}
