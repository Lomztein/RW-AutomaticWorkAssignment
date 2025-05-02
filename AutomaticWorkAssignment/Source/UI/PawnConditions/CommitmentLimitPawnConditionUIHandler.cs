using Lomzie.AutomaticWorkAssignment.PawnConditions;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public class CommitmentLimitPawnConditionUIHandler : PawnSettingUIHandler<CommitmentLimitPawnCondition>
    {
        private static readonly float _inputFieldSize = 32;
        private static string _buffer;

        protected override float Handle(Vector2 position, float width, CommitmentLimitPawnCondition pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _inputFieldSize));
            _buffer = (pawnSetting.Limit * 100f).ToString();
            Widgets.TextFieldNumeric(rect, ref pawnSetting.Limit, ref _buffer);
            pawnSetting.Limit = float.Parse(_buffer) / 100f;
            return _inputFieldSize;
        }
    }
}
