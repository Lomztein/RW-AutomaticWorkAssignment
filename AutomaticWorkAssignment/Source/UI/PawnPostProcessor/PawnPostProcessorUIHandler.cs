using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public class PawnPostProcessorUIHandler<T> : IPawnPostProcessorUIHandler where T : IPawnPostProcessor
    {
        private const float _labelSize = 24;

        public virtual bool CanHandle(IPawnPostProcessor pawnFitness)
            => typeof(T).IsInstanceOfType(pawnFitness);

        public virtual float Handle(Vector2 position, float width, IPawnPostProcessor pawnPostProcessor)
        {
            Rect labelRect = new Rect(position, new Vector2(width, _labelSize));
            labelRect.x += 2;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, pawnPostProcessor.Label);
            Text.Anchor = TextAnchor.UpperLeft;
            return labelRect.height;
        }
    }
}
