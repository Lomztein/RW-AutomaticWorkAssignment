using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.Source.UI.PawnFitness
{
    public class ConstantPawnFitnessUIHandler : PawnSettingUIHandler<ConstantPawnFitness>
    {
        private static readonly float _inputFieldSize = 32;
        private static string _buffer;

        protected override float Handle(Vector2 position, float width, ConstantPawnFitness pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _inputFieldSize));
            _buffer = pawnSetting.Value.ToString();
            Widgets.TextFieldNumeric(rect, ref pawnSetting.Value, ref _buffer);
            pawnSetting.Value = float.Parse(_buffer);
            return _inputFieldSize;
        }
    }
}
