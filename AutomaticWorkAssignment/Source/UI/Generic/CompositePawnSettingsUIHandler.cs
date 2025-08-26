using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using RimWorld;
using System;
using System.Collections;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Generic
{
    public class CompositePawnSettingsUIHandler<TSettingCategory, TSetting, TSettingDef> : IPawnSettingUIHandler
        where TSettingCategory : IPawnSetting where TSetting : IPawnSetting where TSettingDef : PawnSettingDef
    {
        public virtual Action? HelpHandler(IPawnSetting setting) => setting.Def.documentationPath == null ?
            null :
            () => AutomaticWorkAssignmentMod.OpenWebDocumentation(setting.Def.documentationPath);

        public bool CanHandle(IPawnSetting pawnSetting)
            => pawnSetting is TSetting;

        public float Handle(Vector2 position, float width, IPawnSetting pawnSetting)
            => Handle(position, width, (CompositePawnSetting<TSettingCategory>)pawnSetting);

        public string NewSettingLabel = "AWA.NewSettingAddDefault".Translate();
        public bool AllowMoveSettings = true;

        public CompositePawnSettingsUIHandler(string newSettingLabel, bool allowMoveSetting)
        {
            NewSettingLabel = newSettingLabel;
            AllowMoveSettings = allowMoveSetting;
        }

        protected virtual float Handle(Vector2 position, float width, CompositePawnSetting<TSettingCategory> pawnSetting)
        {
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0).Pad(left: 8), GetHashCode(), new(0, 0));

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

                WorkManagerWindow.AddFunctionButton<TSettingCategory, TSettingDef>(
                    ref layout,
                    NewSettingLabel,
                    GetNewSettingAction(pawnSetting),
                    pawnSetting.InnerSettings);
            }

            return layout.Rect.height;
        }

        private Action<TSettingCategory> GetNewSettingAction(CompositePawnSetting<TSettingCategory> pawnSetting)
        {
            if (pawnSetting.InnerSettings.Count != pawnSetting.MaxSettings)
                return (x) => Find.Root.StartCoroutine(DelayedAdd(pawnSetting, x));
            return null;
        }

        private Action<TSettingCategory, int> GetMoveAction(CompositePawnSetting<TSettingCategory> pawnSetting)
        {
            if (AllowMoveSettings)
                return (x, movement) => Find.Root.StartCoroutine(DelayedMove(pawnSetting, x, movement));
            return null;
        }

        private IEnumerator DelayedAdd(CompositePawnSetting<TSettingCategory> composite, TSettingCategory newSetting)
        {
            yield return new WaitForEndOfFrame();
            composite.InnerSettings.Add(newSetting);
        }

        private IEnumerator DelayedMove(CompositePawnSetting<TSettingCategory> composite, TSettingCategory setting, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(composite.InnerSettings, setting, movement);
        }

        private IEnumerator DelayedDelete(CompositePawnSetting<TSettingCategory> composite, TSettingCategory setting)
        {
            yield return new WaitForEndOfFrame();
            composite.InnerSettings.Remove(setting);
        }
    }
}
