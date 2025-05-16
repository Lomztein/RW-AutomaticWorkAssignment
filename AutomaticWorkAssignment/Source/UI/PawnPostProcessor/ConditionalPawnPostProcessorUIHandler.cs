using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI;
using Lomzie.AutomaticWorkAssignment.UI.Generic;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                y += DrawNewSettingButton<PawnConditionDef, IPawnCondition>(innerPosition, width, "Set condition", x => pawnSetting.Condition = x);

            innerPosition = position;
            innerPosition.y += y;

            if (pawnSetting.PostProcessor != null)
                y += DrawSetting(innerPosition, width, pawnSetting.PostProcessor, x => pawnSetting.PostProcessor = null);
            else
                y += DrawNewSettingButton<PawnPostProcessorDef, IPawnPostProcessor>(innerPosition, width, "Set task", x => pawnSetting.PostProcessor = x);

            return y;
        }

        private float DrawSetting(Vector2 position, float width, IPawnSetting setting, Action<IPawnSetting> onDelete)
        {
            float y = 0f;
            position.x += 4;
            width -= 4;

                Rect labelRect = new Rect(position, new Vector2(width, _labelSize));
                (Rect label, Rect button) = Utils.GetLabeledContentWithFixedLabelSize(labelRect, width - _labelSize);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(label, WorkManagerWindow.GetSettingLabel(setting));
                position.y += _labelSize;
                y += _labelSize;
                Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(button, "X"))
            {
                onDelete(setting);
            }
            else
            {
                y += PawnSettingUIHandlers.Handle(position, width, setting);
            }

            return y;
        }

        private float DrawNewSettingButton<TDef, TSetting>(Vector2 position, float width, string newLabel, Action<TSetting> onNewSetting) where TDef : PawnSettingDef where TSetting : IPawnSetting
        {
            Rect buttonRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, newLabel))
            {
                FloatMenuUtility.MakeMenu(DefDatabase<TDef>.AllDefs, x => x.label, x => () => onNewSetting(PawnSetting.CreateFrom<TSetting>(x)));
            }
            return _buttonSize;
        }
    }
}
