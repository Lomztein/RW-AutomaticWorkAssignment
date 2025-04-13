using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetTitlePawnPostProcessor : IPawnPostProcessor
    {
        public string Label => "Assign title";
        public string Description => "Assign title to pawn, defaults to work specifications name.";

        public string Title = string.Empty;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            string title = string.IsNullOrEmpty(Title) ? workSpecification.Name : Title;
            pawn.story.title = title;
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Title, "title", defaultValue: string.Empty);
        }
    }
}
