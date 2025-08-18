using Lomzie.AutomaticWorkAssignment.PawnFitness;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public class ConstantPawnFitnessUIHandler : PawnSettingUIHandler<ConstantPawnFitness>
    {
        private float Size => AutomaticWorkAssignmentSettings.UILabelSizeBase;
        private readonly Buffer<string> _buffer = new Buffer<string>();

        protected override float Handle(Vector2 position, float width, ConstantPawnFitness pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, Size));
            string buffer = _buffer.Get(pawnSetting);
            Widgets.TextFieldNumeric(rect, ref pawnSetting.Value, ref buffer);
            _buffer.Set(pawnSetting, buffer);
            return Size;
        }
    }
}
