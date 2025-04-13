using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.UI.PawnFitness;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.Source.UI.PawnConditions
{
    // TODO: Look into options for unifying UI handlers.

    public class GenericPickerPawnFitnessUIHandler<T, E> : PawnFitnessUIHandler<T> where T : IPawnFitness
    {
        private readonly Func<IEnumerable<E>> _optionGetter;
        private readonly Func<E, string> _optionLabelGetter;
        private readonly Func<T, string> _labelGetter;
        private readonly Action<T, E> _onSelected;

        private readonly float _pickButtonSize = 32;

        public GenericPickerPawnFitnessUIHandler(Func<IEnumerable<E>> optionGetter, Func<E, string> optionLabelGetter, Func<T, string> labelGetter, Action<T, E> onSelected)
        {
            _optionGetter = optionGetter;
            _optionLabelGetter = optionLabelGetter;
            _labelGetter = labelGetter;
            _onSelected = onSelected;
        }

        public override float Handle(Vector2 position, float width, IPawnFitness pawnFitness)
        {
            T genericPawnFitness = (T)pawnFitness;
            float y = base.Handle(position, width, pawnFitness);
            Rect buttonRect = new Rect(position, new Vector2(width, _pickButtonSize));
            buttonRect.y += y;
            if (Widgets.ButtonText(buttonRect, _labelGetter(genericPawnFitness)))
            {
                FloatMenuUtility.MakeMenu(_optionGetter(), _optionLabelGetter, x => () => _onSelected(genericPawnFitness, x));
            }

            y += buttonRect.height;
            return y;
        }
    }
}
