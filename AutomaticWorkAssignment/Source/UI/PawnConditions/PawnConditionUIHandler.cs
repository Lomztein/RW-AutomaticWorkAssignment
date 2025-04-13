using Lomzie.AutomaticWorkAssignment.PawnConditions;
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
    public class PawnConditionUIHandler<T> : IPawnConditionUIHandler where T : IPawnCondition
    {
        private const float _labelSize = 24;

        public virtual bool CanHandle(IPawnCondition pawnFitness)
            => typeof(T).IsInstanceOfType(pawnFitness);

        public virtual float Handle(Vector2 position, float width, IPawnCondition pawnCondition)
        {
            Rect labelRect = new Rect(position, new Vector2(width, _labelSize));
            labelRect.x += 2;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, pawnCondition.Label);
            Text.Anchor = TextAnchor.UpperLeft;
            return labelRect.height;
        }
    }
}
