using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public class SetTitlePawnPostProcessorUIHandler : PawnPostProcessorUIHandler<SetTitlePawnPostProcessor>
    {
        private float _inputFieldSize = 24;
        private string _buffer;

        public override float Handle(Vector2 position, float width, IPawnPostProcessor pawnPostProcessor)
        {
            SetTitlePawnPostProcessor titlePostProcessor = (SetTitlePawnPostProcessor)pawnPostProcessor;

            float y = base.Handle(position, width, pawnPostProcessor);
            Rect inputRect = new Rect(position, new Vector2(width, _inputFieldSize));
            inputRect.y += y;

            titlePostProcessor.Title = Widgets.TextField(inputRect, titlePostProcessor.Title);
            y += inputRect.height;
            return y;
        }
    }
}
