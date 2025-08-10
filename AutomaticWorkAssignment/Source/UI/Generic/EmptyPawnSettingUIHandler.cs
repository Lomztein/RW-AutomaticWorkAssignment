using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.UI;
using UnityEngine;

namespace AutomaticWorkAssignment.UI.Generic
{
    public class EmptyPawnSettingUIHandler<T> : PawnSettingUIHandler<T> where T : IPawnSetting
    {
        protected override float Handle(Vector2 position, float width, T pawnSetting)
        {
            return 0f;
        }
    }
}
