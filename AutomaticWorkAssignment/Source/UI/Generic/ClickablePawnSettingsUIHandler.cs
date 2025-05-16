using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Generic
{
    public class ClickablePawnSettingsUIHandler<T> : PawnSettingUIHandler<T> where T : IPawnSetting
    {
        private Action<T> _onClick;
        private string _label;

        private readonly float _buttonSize = 32;

        public ClickablePawnSettingsUIHandler (Action<T> onClick, string label)
        {
            _onClick = onClick;
            _label = label;
        }

        protected override float Handle(Vector2 position, float width, T pawnSetting)
        {
            Rect buttonRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, _label))
            {
                _onClick(pawnSetting);
            }
            return _buttonSize;
        }
    }
}
