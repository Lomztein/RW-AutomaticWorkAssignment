using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public class PawnFitnessUIHandler<T> : IPawnFitnessUIHandler where T : IPawnFitness
    {
        private const float _labelSize = 24;

        public virtual bool CanHandle(IPawnFitness pawnFitness)
            => typeof(T).IsInstanceOfType(pawnFitness);

        public virtual float Handle(Vector2 position, float width, IPawnFitness pawnFitness)
        {
            Rect labelRect = new Rect(position, new Vector2(width, _labelSize));
            labelRect.x += 2;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, pawnFitness.Label);
            Text.Anchor = TextAnchor.UpperLeft;
            return labelRect.height;
        }
    }
}
