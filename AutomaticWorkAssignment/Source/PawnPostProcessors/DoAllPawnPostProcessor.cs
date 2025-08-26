using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class DoAllPawnPostProcessor : CompositePawnSetting<IPawnPostProcessor>, IPawnPostProcessor, ICompositePawnSetting
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            foreach (IPawnPostProcessor postProcessor in InnerSettings)
            {
                postProcessor.PostProcess(pawn, workSpecification, request);
            }
        }
    }
}
