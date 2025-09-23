using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class DedicatePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public enum TimeUnit { Hour, Day, Week, Month, Year }

        public float Time = 1;
        public TimeUnit Unit = TimeUnit.Month;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            request.WorkManager.Dedications.Dedicate(pawn, workSpecification, CalcExpirationTick(Time, Unit));
        }

        private int CalcExpirationTick(float time, TimeUnit unit)
        {
            int ticksPerUnit = GetTicksPerUnit(unit);
            return GenTicks.TicksAbs + (int)(time * ticksPerUnit);
        }

        private WorkSpecification GetWorkSpec()
        {
            MapWorkManager manager = MapWorkManager.GetCurrentMapManager();
            WorkSpecification spec = manager.WorkList.FirstOrDefault(x => Utils.FindRecursive(x.PostProcessors, s => s == this).Any());
            return spec;
        }

        public IEnumerable<Pawn> GetCurrentMapDedicatedPawns ()
        {
            MapWorkManager manager = MapWorkManager.GetCurrentMapManager();
            WorkSpecification spec = GetWorkSpec();
            if (spec != null)
                return manager.Dedications.GetDedicatedPawns(spec);
            return Enumerable.Empty<Pawn>();
        }

        public void ClearCurrentMapDedicatedPawns()
        {
            MapWorkManager manager = MapWorkManager.GetCurrentMapManager();
            WorkSpecification spec = GetWorkSpec();
            if (spec != null)
                manager.Dedications.ClearDedications(spec);
        }

        private int GetTicksPerUnit(TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Hour:
                    return GenDate.TicksPerHour;
                case TimeUnit.Day:
                    return GenDate.TicksPerDay;
                case TimeUnit.Week:
                    return GenDate.TicksPerDay * 5;
                case TimeUnit.Month:
                    return GenDate.TicksPerQuadrum;
                case TimeUnit.Year:
                    return GenDate.TicksPerYear;
                default:
                    break;
            }
            throw new ArgumentException($"Unknown time unit {unit}");
        }

        public static string GetLabel(TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Hour:
                    return "AWA.Hours";
                case TimeUnit.Day:
                    return "AWA.Days";
                case TimeUnit.Week:
                    return "AWA.Weeks";
                case TimeUnit.Month:
                    return "AWA.Months";
                case TimeUnit.Year:
                    return "AWA.Years";
                default:
                    break;
            }
            throw new ArgumentException($"Unknown time unit {unit}");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Time, "time", defaultValue: Time);
            Scribe_Values.Look(ref Unit, "unit", defaultValue: Unit);
        }
    }
}
