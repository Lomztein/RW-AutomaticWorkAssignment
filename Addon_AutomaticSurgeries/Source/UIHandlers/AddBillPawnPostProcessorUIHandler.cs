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

                // First search for therotically valid body parts for recipe.
                var validBodyParts = pawnPostProcessor.GetTheoreticallyValidBodyPartsFor();

                // Concatonate with any we might have missed on current map.
                validBodyParts = validBodyParts.Concat(pawnPostProcessor.GetValidBodyPartsForOnMap(Find.CurrentMap));

                // Filter out potential duplicates
                validBodyParts = validBodyParts.Distinct();

                if (pawnPostProcessor.BodyPartRecord != null)
                    pawnPostProcessor.BodyPartRecord = validBodyParts.FirstOrDefault(x => x.LabelCap == pawnPostProcessor.BodyPartRecord?.LabelCap);

                if (Widgets.ButtonText(bodyPartRect, pawnPostProcessor.BodyPartRecord?.LabelCap ?? "AWA.Auto".Translate()))
                {
                    SearchableFloatMenu.MakeMenu(new BodyPartRecord[] { null }.Concat(validBodyParts), x => x?.LabelCap ?? "AWA.Auto".Translate(), x => () => pawnPostProcessor.BodyPartRecord = x);
                }
                y += _buttonSize;
            }
            return y;
        }
    }
}
