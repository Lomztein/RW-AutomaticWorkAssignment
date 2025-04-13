using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public interface IPawnFitnessUIHandler
    {
        bool CanHandle(IPawnFitness pawnFitness);

        float Handle(Vector2 position, float width, IPawnFitness pawnFitness);
    }
}
