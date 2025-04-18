﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    /// <summary>
    /// Represents a pawns fitness for a role. Higher fitness = better match.
    /// </summary>
    public interface IPawnFitness : IPawnSetting
    {
        float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request);
    }
}
