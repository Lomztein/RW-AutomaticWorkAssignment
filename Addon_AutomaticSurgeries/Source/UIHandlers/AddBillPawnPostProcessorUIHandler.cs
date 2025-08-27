using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor
{
    public class AddBillPawnPostProcessorUIHandler : PawnSettingUIHandler<AddBillPawnPostProcessor>
    {
        private readonly float _buttonSize = 32;

        protected override float Handle(Vector2 position, float width, AddBillPawnPostProcessor pawnPostProcessor)
        {
            float y = 0;
            Rect recipeRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(recipeRect, pawnPostProcessor.BillRecipeDef?.LabelCap ?? "AWA.OperationSelect".Translate()))
            {
                SearchableFloatMenu.MakeMenu(AddBillPawnPostProcessor.GetValidRecipies(), x => x.LabelCap, x => () => pawnPostProcessor.BillRecipeDef = x);
            }
            y += _buttonSize;
            if (pawnPostProcessor.BillRecipeDef != null && pawnPostProcessor.BillRecipeDef.targetsBodyPart)
            {
                position.y += _buttonSize;
                Rect bodyPartRect = new Rect(position, new Vector2(width, _buttonSize));
                var validBodyParts = AddBillPawnPostProcessor.GetValidBodyPartsFor(pawnPostProcessor.BillRecipeDef);
                if (validBodyParts.Count() == 1)
                {
                    pawnPostProcessor.BodyPartRecord = validBodyParts.FirstOrDefault();
                }

                if (Widgets.ButtonText(bodyPartRect, pawnPostProcessor.BodyPartRecord?.LabelCap ?? "AWA.BodyPartSelect".Translate()))
                {
                    SearchableFloatMenu.MakeMenu(validBodyParts, x => x.LabelCap, x => () => pawnPostProcessor.BodyPartRecord = x);
                }
                y += _buttonSize;
            }
            return y;
        }
    }
}
