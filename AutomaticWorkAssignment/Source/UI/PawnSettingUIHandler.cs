﻿using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public abstract class PawnSettingUIHandler<T> : IPawnSettingUIHandler where T : IPawnSetting
    {
        public virtual bool CanHandle(IPawnSetting pawnSetting)
            => typeof(T).IsInstanceOfType(pawnSetting);

        public float Handle(Vector2 position, float width, IPawnSetting pawnSetting) =>
            Handle(position, width, (T)pawnSetting);

        protected abstract float Handle(Vector2 position, float width, T pawnSetting);
    }
}
