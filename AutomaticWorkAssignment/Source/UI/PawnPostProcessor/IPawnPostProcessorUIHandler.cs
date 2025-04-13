using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public interface IPawnPostProcessorUIHandler
    {
        bool CanHandle(IPawnPostProcessor pawnFitness);

        float Handle(Vector2 position, float width, IPawnPostProcessor pawnFitness);
    }
}
