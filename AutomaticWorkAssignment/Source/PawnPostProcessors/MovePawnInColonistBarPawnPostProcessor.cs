using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class MovePawnInColonistBarPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public enum MoveToSide { Left, Right };
        public MoveToSide MoveTo;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.playerSettings != null)
            {
                int offset = request.GetVariable<int>("MovePawnInColonistBarOffset");

                int sign = MoveTo == MoveToSide.Left ? -1 : 1;
                pawn.playerSettings.displayOrder = ((int.MaxValue / 2) - offset) * sign;

                request.SetVariable("MovePawnInColonistBarOffset", offset + 1);
                Find.ColonistBar.MarkColonistsDirty();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MoveTo, "moveTo");
        }

        public static IEnumerable<MoveToSide> GetOptions()
            => Enum.GetValues(typeof(MoveToSide)).Cast<MoveToSide>();

        public static string GetLabel(MoveToSide? moveTo)
        {
            if (!moveTo.HasValue)
                return null;

            switch (moveTo)
            {
                case MoveToSide.Left:
                    return "AWA.Left".Translate();
                case MoveToSide.Right:
                    return "AWA.Right".Translate();
            }
            return null;
        }
    }
}
