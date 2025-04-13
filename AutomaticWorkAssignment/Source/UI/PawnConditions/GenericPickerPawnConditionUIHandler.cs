using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.PawnFitness;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.Source.UI.PawnConditions
{
    public class GenericPickerPawnConditionUIHandler<T, E> : PawnConditionUIHandler<T> where T : IPawnCondition
    {
        private readonly Func<IEnumerable<E>> _optionGetter;
        private readonly Func<E, string> _optionLabelGetter;
        private readonly Func<T, string> _labelGetter;
        private readonly Action<T, E> _onSelected;

        private readonly float _pickButtonSize = 32;

        public GenericPickerPawnConditionUIHandler(Func<IEnumerable<E>> optionGetter, Func<E, string> optionLabelGetter, Func<T, string> labelGetter, Action<T, E> onSelected)
        {
            _optionGetter = optionGetter;
            _optionLabelGetter = optionLabelGetter;
            _labelGetter = labelGetter;
            _onSelected = onSelected;
        }

        public override float Handle(Vector2 position, float width, IPawnCondition pawnCondition)
        {
            T genericPawnCondition = (T)pawnCondition;
            float y = base.Handle(position, width, pawnCondition);
            Rect buttonRect = new Rect(position, new Vector2(width, _pickButtonSize));
            buttonRect.y += y;

            if (Widgets.ButtonText(buttonRect, _labelGetter(genericPawnCondition)))
            {
                FloatMenuUtility.MakeMenu(_optionGetter(), _optionLabelGetter, x => () => _onSelected(genericPawnCondition, x));
            }

            y += buttonRect.height;
            return y;
        }
    }
}
