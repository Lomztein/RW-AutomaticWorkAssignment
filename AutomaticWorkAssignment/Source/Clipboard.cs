using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class Clipboard
    {
        private static object _clipboardObject;
        private static Type _objectType;

        // Using IO to basically do a deep-copy of any exposable is somewhat hacky, but beats manually cloning everything.
        public static void Copy(IExposable obj)
        {
            IO.ExportToFile(obj, "clipboard", IO.GetClipboardDirectory());
            _objectType = obj.GetType();

            IExposable clipboardObj = Activator.CreateInstance(_objectType) as IExposable;
            IO.ImportFromFile(clipboardObj, "clipboard", IO.GetClipboardDirectory());

            _clipboardObject = clipboardObj;
        }

        public static bool Contains<T> ()
            => _objectType.IsAssignableFrom(typeof(T));

        public static T Get<T>()
        {
            if (_objectType.IsAssignableFrom(typeof(T)))
            {
                return (T)_clipboardObject;
            }
            return default;
        }
    }
}
