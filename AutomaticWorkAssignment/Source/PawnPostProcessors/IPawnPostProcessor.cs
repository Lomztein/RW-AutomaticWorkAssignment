using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public interface IPawnPostProcessor : IPawnSetting
    {
        void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request);
    }
}
