﻿using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public class ConditionalPawnPostProcessorUIHandler : PawnSettingUIHandler<ConditionalPawnPostProcessor>
    {
        private readonly float _labelSize = 24;
        private readonly float _buttonSize = 32;

        protected override float Handle(Vector2 position, float width, ConditionalPawnPostProcessor pawnSetting)
        {
            float y = 0;
            Vector2 innerPosition = position;
            if (pawnSetting.Condition != null)
                y += DrawSetting(innerPosition, width, pawnSetting.Condition, x => pawnSetting.Condition = null);
            else
                y += DrawNewSettingButton<PawnConditionDef, IPawnCondition>(innerPosition, width, "AWA.ConditionSelect".Translate(), x => pawnSetting.Condition = x);

            innerPosition = position;
            innerPosition.y += y;

            if (pawnSetting.PostProcessor != null)
                y += DrawSetting(innerPosition, width, pawnSetting.PostProcessor, x => pawnSetting.PostProcessor = null);
            else
                y += DrawNewSettingButton<PawnPostProcessorDef, IPawnPostProcessor>(innerPosition, width, "AWA.PostProcessorSelect".Translate(), x => pawnSetting.PostProcessor = x);

            return y;
        }

        private float DrawSetting(Vector2 position, float width, IPawnSetting setting, Action<IPawnSetting> onDelete)
        {
            position.x += 4;
            width -= 4;

            return WorkManagerWindow.DoPawnSetting(position, width, setting, canMoveUp: false, canMoveDown: false, null, onDelete);
        }

        private float DrawNewSettingButton<TDef, TSetting>(Vector2 position, float width, string newLabel, Action<TSetting> onNewSetting) where TDef : PawnSettingDef where TSetting : IPawnSetting
        {
            Rect buttonRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, newLabel))
            {
                FloatMenuUtility.MakeMenu(DefDatabase<TDef>.AllDefs, x => x.LabelCap, x => () => onNewSetting(PawnSetting.CreateFrom<TSetting>(x)));
            }
            return _buttonSize;
        }
    }
}
