using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        Dictionary<CompositePawnSetting, ListData> _lastData = new Dictionary<CompositePawnSetting, ListData>();

        private float _sectionHeight = 128;

        public string NewSettingLabel = "Add";
        public bool AllowMoveSettings = true;

        public CompositePawnSettingsUIHandler(string newSettingLabel, bool allowMoveSetting)
        {
            NewSettingLabel = newSettingLabel;
            AllowMoveSettings = allowMoveSetting;
        }

        protected virtual float Handle(Vector2 position, float width, CompositePawnSetting pawnSetting)
        {
            float y = 0f;
            Vector2 innerPosition = position;

            innerPosition.x += 4;
            float innerWidth = width - 4;
            if (pawnSetting.InnerSettings != null)
            {
                Rect sectionRect = new Rect(innerPosition, new Vector2(innerWidth, _sectionHeight));
                ListData listData = GetListData(pawnSetting);

                WorkManagerWindow.DoPawnSettingList(sectionRect, typeof(D), NewSettingLabel, ref listData.Height, ref listData.Position,
                    () => pawnSetting.InnerSettings,
                    GetNewSettingAction(pawnSetting),
                    GetMoveAction(pawnSetting),
                    (x) => Find.Root.StartCoroutine(DelayedDelete(pawnSetting, x)));

                y += Mathf.Min(listData.Height, _sectionHeight);
            }

            return y;
        }

        private Action<IPawnSetting> GetNewSettingAction (CompositePawnSetting pawnSetting)
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

        private ListData GetListData(CompositePawnSetting pawnSetting)
        {
            if (!_lastData.ContainsKey(pawnSetting))
            {
                _lastData.Add(pawnSetting, new ListData());
            }
            return _lastData[pawnSetting];
        }

        private class ListData
        {
            public float Height;
            public Vector2 Position;
        }
    }
}
