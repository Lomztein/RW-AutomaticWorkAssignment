using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public interface IHandlerModule<T> where T : IPawnSetting
    {
        float Handle(Vector2 position, float width, T pawnSetting);
    }
}
