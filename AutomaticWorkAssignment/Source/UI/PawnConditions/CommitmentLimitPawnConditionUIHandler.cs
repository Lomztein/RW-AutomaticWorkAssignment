using Lomzie.AutomaticWorkAssignment.PawnConditions;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public class CommitmentLimitPawnConditionUIHandler : PawnSettingUIHandler<CommitmentLimitPawnCondition>
    {
        private static readonly float _inputFieldSize = 32;
        private Buffer<string> _buffer = new Buffer<string>();

        protected override float Handle(Vector2 position, float width, CommitmentLimitPawnCondition pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _inputFieldSize));
            float percentage = pawnSetting.Limit * 100;
            string buffer = _buffer.Get(pawnSetting);
            Widgets.TextFieldNumeric(rect, ref percentage, ref buffer);
            pawnSetting.Limit = percentage / 100f;
            _buffer.Set(pawnSetting, buffer);
            return _inputFieldSize;
        }
    }
}
