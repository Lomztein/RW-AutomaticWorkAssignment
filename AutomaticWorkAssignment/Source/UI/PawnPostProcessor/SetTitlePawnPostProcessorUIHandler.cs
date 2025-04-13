using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public class SetTitlePawnPostProcessorUIHandler : PawnSettingUIHandler<SetTitlePawnPostProcessor>
    {
        private float _inputFieldSize = 24;

        protected override float Handle(Vector2 position, float width, SetTitlePawnPostProcessor pawnPostProcessor)
        {
            Rect inputRect = new Rect(position, new Vector2(width, _inputFieldSize));
            pawnPostProcessor.Title = Widgets.TextField(inputRect, pawnPostProcessor.Title);
            return inputRect.height;
        }
    }
}
