using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class PawnFitnessComparer : IComparer<Pawn>
    {
        private readonly List<IPawnFitness> _fitnessFunctions;
        private readonly WorkSpecification _workSpec;
        private readonly ResolveWorkRequest _workReq;

        public PawnFitnessComparer(List<IPawnFitness> funcs, WorkSpecification workSpec, ResolveWorkRequest workReq)
        {
            _fitnessFunctions = funcs;
            _workSpec = workSpec;
            _workReq = workReq;
        }

        public int Compare(Pawn x, Pawn y)
        {
            float diff = 0f;

            for (int i = 0; i < _fitnessFunctions.Count; i++)
            {
                float xFitnessd = _fitnessFunctions[i].CalcFitness(x, _workSpec, _workReq);
                float yFitnessd = _fitnessFunctions[i].CalcFitness(y, _workSpec, _workReq);
                diff = yFitnessd - xFitnessd;

                if (Math.Abs(diff) > Mathf.Epsilon)
                    break;
            }

            return (int)Mathf.Sign(diff);
        }
    }
}
