using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public class SetCarriedMedicinePawnPostProcessorUIHandler : PawnSettingUIHandler<SetCarriedMedicinePawnPostProcessor>
    {
        private static readonly float _inputFieldSize = 32;
        private static string _buffer;

        protected override float Handle(Vector2 position, float width, SetCarriedMedicinePawnPostProcessor pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _inputFieldSize));
            _buffer = pawnSetting.Count.ToString();
            Widgets.TextFieldNumeric(rect, ref pawnSetting.Count, ref _buffer);
            pawnSetting.Count = int.Parse(_buffer);
            return _inputFieldSize;
        }
    }
}
