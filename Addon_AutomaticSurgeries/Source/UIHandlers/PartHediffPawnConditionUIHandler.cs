using Lomzie.AutomaticWorkAssignment.PawnConditions;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public class PartHediffPawnConditionUIHandler : PawnSettingUIHandler<PartHediffPawnCondition>
    {
        private readonly float _buttonSize = 32;

        protected override float Handle(Vector2 position, float width, PartHediffPawnCondition pawnPostProcessor)
        {
            float y = 0;
            Rect recipeRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(recipeRect, pawnPostProcessor.HediffDef?.LabelCap ?? "AWA.ConditionSelect".Translate()))
            {
                FloatMenuUtility.MakeMenu(DefDatabase<HediffDef>.AllDefs, x => x.LabelCap, x => () => pawnPostProcessor.HediffDef = x);
            }
            y += _buttonSize;
            position.y += _buttonSize;
            Rect bodyPartRect = new Rect(position, new Vector2(width, _buttonSize));
            var validBodyParts = BodyDefOf.Human.AllParts;
            if (validBodyParts.Count() == 1)
            {
                pawnPostProcessor.HediffPart = validBodyParts.FirstOrDefault();
            }
            if (pawnPostProcessor.HediffPart != null && !validBodyParts.Any(x => x.Index == pawnPostProcessor.HediffPart.Index))
            {
                pawnPostProcessor.HediffPart = null;
            }

            if (Widgets.ButtonText(bodyPartRect, pawnPostProcessor.HediffPart?.LabelCap ?? "AWA.BodyPartSelect".Translate()))
            {
                FloatMenuUtility.MakeMenu(validBodyParts, x => x.LabelCap, x => () => pawnPostProcessor.HediffPart = x);
            }
            y += _buttonSize;
            return y;
        }
    }
}
