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
        private static IExposable _clipboardObject;
        private static Type _objectType;

        // Using IO to basically do a deep-copy of any exposable is somewhat hacky, but I'm lazy and this beats manually cloning everything.
        public static void Copy(IExposable obj)
        {
            _objectType = obj.GetType();
            _clipboardObject = MakeCopy(obj);
        }

        private static IExposable MakeCopy (IExposable obj)
        {
            IO.ExportToFile(obj, "clipboard", IO.GetClipboardDirectory());
            IExposable copy = Activator.CreateInstance(_objectType) as IExposable;
            IO.ImportFromFile(copy, "clipboard", IO.GetClipboardDirectory());
            return copy;
        }

        public static bool ContainsAny()
            => _clipboardObject != null && _objectType != null;

        public static bool Contains<T>()
            => Contains(typeof(T));

        public static bool Contains(Type type)
        {
            if (!ContainsAny())
                return false;
            return type.IsAssignableFrom(_objectType);
        }

        public static IExposable Paste(Type expectedType)
        {
            if (!ContainsAny())
                throw new InvalidOperationException("No object in clipboard.");

            if (expectedType.IsAssignableFrom(_objectType))
            {
                return MakeCopy(_clipboardObject);
            }
            return default;
        }

        public static T Paste<T>() where T : IExposable
            => (T)Paste(typeof(T));

        public static void PasteInto(IExposable exposable)
        {
            if (!ContainsAny())
                throw new InvalidOperationException("No object in clipboard.");

            IO.ExportToFile(_clipboardObject, "clipboard", IO.GetClipboardDirectory());
            IO.ImportFromFile(exposable, "clipboard", IO.GetClipboardDirectory());
        }

        public static void Clear ()
        {
            _clipboardObject = null;
            _objectType = null;
        }
    }
}
