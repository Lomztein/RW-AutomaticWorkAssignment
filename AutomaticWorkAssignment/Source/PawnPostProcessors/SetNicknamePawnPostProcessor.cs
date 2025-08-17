using System;
using System.Collections.Generic;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetNicknamePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public string Format = string.Empty;
        
        private Dictionary<string, Func<Pawn, string>> _getters = new Dictionary<string, Func<Pawn, string>>()
        {
            { "first", x => (x.Name is NameTriple triple ? triple.First : (x.Name as NameSingle).Name) },
            { "last", x => x.Name is NameTriple triple ? triple.Last : "" },
            { "title", x => x.story.Title },
        };
        private IEnumerable<string> Keywords => _getters.Keys;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            string newNickname = ResolveNickname(Format, pawn);
            pawn.Name = BuildName(pawn, newNickname);
        }

        private string ResolveNickname(string format, Pawn pawn)
        {
            string nickname = format;
            foreach (var keyword in Keywords)
            {
                string lookFor = "{" + keyword + "}";
                nickname = nickname.Replace(lookFor, _getters[keyword](pawn));
            }
            return nickname;
        }

        private Name BuildName(Pawn pawn, string newNickname)
        {
            string first = string.Empty;
            string last = string.Empty;
            string nick = newNickname;

            if (pawn.Name is NameTriple triple)
            {
                first = triple.First;
                last = triple.Last;
            }

            if (pawn.Name is NameSingle single)
            {
                first = single.Name;
                last = string.Empty;
            }

            return new NameTriple(first, nick, last);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Format, "title", defaultValue: string.Empty);
        }
    }
}
