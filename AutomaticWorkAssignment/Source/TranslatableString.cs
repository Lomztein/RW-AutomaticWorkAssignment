using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public struct TranslatableString
    {
        public readonly string Key;
        public readonly NamedArgument[] Params;
        public readonly bool Translatable;

        private TranslatableString(string key, NamedArgument[] @params, bool translatable)
        {
            Key = key;
            Params = @params;
            Translatable = translatable;
        }
        public TranslatableString(string key, Dictionary<string, object> @params) : this(key, @params.Select(kvp => new NamedArgument(kvp.Value, kvp.Key)).ToArray(), true) { }
        public TranslatableString(string key, params NamedArgument[] @params) : this(key, @params, true) { }

        public readonly string Translate()
        {
            if (Translatable && LanguageDatabase.activeLanguage != null)
            {
                return Key.Translate(Params);
            }
            return Key + (Params.Length > 0 ? " " + string.Join(" ", Params.Select(param => param.ToString()).ToArray()) : "");
        }

        public static implicit operator TranslatableString(string key) => new(key, new NamedArgument[] { }, false);
    }
}
