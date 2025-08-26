using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class Composite<T, TInnerSetting, D> : IHandlerModule<T>
        where T : IPawnSetting where TInnerSetting : IPawnSetting where D : PawnSettingDef
    {
        private readonly Func<T, IList<TInnerSetting>> _getter;
        private string _newSettingLabel;

        public Composite(Func<T, IList<TInnerSetting>> getter, string newSettingLabel)
        {
            _getter = getter;
            _newSettingLabel = newSettingLabel;
        }

        public float Handle(Vector2 position, float width, T pawnSetting)
        {
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0).Pad(left: 8), GetHashCode(), new(0, 0));
            IList<TInnerSetting> settings = _getter(pawnSetting);

            if (settings != null)
            {
                for (var i = 0; i < settings.Count; i++)
                {
                    var setting = settings[i];
                    WorkManagerWindow.DoPawnSetting(
                        ref layout,
                        setting: setting,
                        canMoveUp: i > 0,
                        canMoveDown: i < settings.Count,
                        onMoveSetting: GetMoveAction(pawnSetting),
                        onDeleteSetting: (x) => Find.Root.StartCoroutine(DelayedDelete(pawnSetting, x)));
                }

                WorkManagerWindow.AddFunctionButton<TInnerSetting, D>(
                    ref layout,
                    _newSettingLabel,
                    GetNewSettingAction(pawnSetting),
                    settings);
            }

            return layout.Rect.height;
        }

        private Action<TInnerSetting> GetNewSettingAction(T setting)
        {
            return (x) => Find.Root.StartCoroutine(DelayedAdd(setting, x));
        }

        private Action<TInnerSetting, int> GetMoveAction(T setting)
        {
            return (x, movement) => Find.Root.StartCoroutine(DelayedMove(setting, x, movement));
        }

        private IEnumerator DelayedAdd(T setting, TInnerSetting newSetting)
        {
            yield return new WaitForEndOfFrame();
            _getter(setting).Add(newSetting);
        }

        private IEnumerator DelayedMove(T setting, TInnerSetting toMove, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(_getter(setting), toMove, movement);
        }

        private IEnumerator DelayedDelete(T setting, TInnerSetting toDelete)
        {
            yield return new WaitForEndOfFrame();
            _getter(setting).Remove(toDelete);
        }
    }
}
