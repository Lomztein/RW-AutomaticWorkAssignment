using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using RimWorld;
using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Generic
{
    public class CompositePawnSettingsUIHandler<T, D> : IPawnSettingUIHandler where T : IPawnSetting where D : PawnSettingDef
    {
        public bool CanHandle(IPawnSetting pawnSetting)
            => pawnSetting is T;

        public float Handle(Vector2 position, float width, IPawnSetting pawnSetting)
            => Handle(position, width, (CompositePawnSetting)pawnSetting);

        private float _sectionHeight = 128;

        public string NewSettingLabel = "AWA.NewSettingAddDefault".Translate();
        public bool AllowMoveSettings = true;

        public CompositePawnSettingsUIHandler(string newSettingLabel, bool allowMoveSetting)
        {
            NewSettingLabel = newSettingLabel;
            AllowMoveSettings = allowMoveSetting;
        }

        protected virtual float Handle(Vector2 position, float width, CompositePawnSetting pawnSetting)
        {
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0).Pad(left: 8), GetHashCode(), new(0, 1));

            if (pawnSetting.InnerSettings != null)
            {
                for (var i = 0; i < pawnSetting.InnerSettings.Count; i++)
                {
                    var setting = pawnSetting.InnerSettings[i];
                    WorkManagerWindow.DoPawnSetting(
                        ref layout,
                        setting: setting,
                        canMoveUp: i > 0,
                        canMoveDown: i < pawnSetting.InnerSettings.Count,
                        onMoveSetting: GetMoveAction(pawnSetting),
                        onDeleteSetting: (x) => Find.Root.StartCoroutine(DelayedDelete(pawnSetting, x)));
                }

                WorkManagerWindow.AddFunctionButton<D>(
                    ref layout,
                    NewSettingLabel,
                    GetNewSettingAction(pawnSetting),
                    pawnSetting.InnerSettings);
            }

            return layout.Rect.height;
        }

        private Action<IPawnSetting> GetNewSettingAction(CompositePawnSetting pawnSetting)
        {
            if (pawnSetting.InnerSettings.Count != pawnSetting.MaxSettings)
                return (x) => Find.Root.StartCoroutine(DelayedAdd(pawnSetting, x));
            return null;
        }

        private Action<IPawnSetting, int> GetMoveAction(CompositePawnSetting pawnSetting)
        {
            if (AllowMoveSettings)
                return (x, movement) => Find.Root.StartCoroutine(DelayedMove(pawnSetting, x, movement));
            return null;
        }

        private IEnumerator DelayedAdd(CompositePawnSetting composite, IPawnSetting newSetting)
        {
            yield return new WaitForEndOfFrame();
            composite.InnerSettings.Add(newSetting);
        }

        private IEnumerator DelayedMove(CompositePawnSetting composite, IPawnSetting setting, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(composite.InnerSettings, setting, movement);
        }

        private IEnumerator DelayedDelete(CompositePawnSetting composite, IPawnSetting setting)
        {
            yield return new WaitForEndOfFrame();
            composite.InnerSettings.Remove(setting);
        }
    }
}
