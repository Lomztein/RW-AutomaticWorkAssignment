using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.UI.Amounts;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class EditPawnSettingsListWindow<TPawnSetting> : Window where TPawnSetting : IPawnSetting
    {
        public override Vector2 InitialSize => new Vector2(300, 400);

        private readonly IList<TPawnSetting> _settings;

        public EditPawnSettingsListWindow(IList<TPawnSetting> settings)
        {
            _settings = settings;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var layout = new RectAggregator(new Rect(inRect.position.x, inRect.position.y, inRect.width, 0).Pad(left: 8), GetHashCode(), new(0, 0));
            Rect headerRect = layout.NewRow(32);
            Commons.DoHeader(headerRect, "AWA.HeaderConditions".Translate());
            if (Widgets.CloseButtonFor(inRect))
            {
                Close();
            }

            if (_settings != null)
            {
                for (var i = 0; i < _settings.Count; i++)
                {
                    var setting = _settings[i];
                    WorkManagerWindow.DoPawnSetting(
                        ref layout,
                        setting: setting,
                        canMoveUp: i > 0,
                        canMoveDown: i < _settings.Count,
                        onMoveSetting: GetMoveAction(),
                        onDeleteSetting: (x) => Find.Root.StartCoroutine(DelayedDelete(x)),
                        onReplaceSetting: (x, newSetting) => Find.Root.StartCoroutine(DelayedReplace(x, newSetting)));

                }

                WorkManagerWindow.DoAddSettingButton<TPawnSetting, PawnConditionDef>(
                    ref layout,
                    "AWA.ConditionAdd".Translate(),
                    GetNewSettingAction(),
                    _settings.Count % 2 == 1);
            }
        }

        private Action<TPawnSetting> GetNewSettingAction()
        {
            return (x) => Find.Root.StartCoroutine(DelayedAdd(x));
        }

        private Action<TPawnSetting, int> GetMoveAction()
        {
            return (x, movement) => Find.Root.StartCoroutine(DelayedMove(x, movement));
        }

        private IEnumerator DelayedAdd(TPawnSetting newSetting)
        {
            yield return new WaitForEndOfFrame();
            _settings.Add(newSetting);
        }

        private IEnumerator DelayedMove(TPawnSetting toMove, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(_settings, toMove, movement);
        }

        private IEnumerator DelayedDelete(TPawnSetting toDelete)
        {
            yield return new WaitForEndOfFrame();
            _settings.Remove(toDelete);
        }

        private IEnumerator DelayedReplace(TPawnSetting original, TPawnSetting newSetting)
        {
            yield return new WaitForEndOfFrame();
            Utils.ReplaceElement(_settings, original, newSetting);
        }
    }
}
