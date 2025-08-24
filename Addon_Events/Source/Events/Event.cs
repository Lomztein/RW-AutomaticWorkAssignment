using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Events
{
    public class Event : IExposable
    {
        public EventDef Def;
        
        public string Label => Def?.LabelCap;
        public string Description => Def?.description;

        private List<PostProcessorRef> _invokeOnEvent = new List<PostProcessorRef>();

        public void Reset () { _invokeOnEvent.Clear(); }
        public bool AnySubscribers() => _invokeOnEvent.Any();

        public void Subscribe(WorkSpecification spec, IPawnPostProcessor postProcessor)
        {
            _invokeOnEvent.Add(new PostProcessorRef(spec, postProcessor));
        }

        public void Invoke(Pawn forPawn, ResolveWorkRequest request)
        {
            foreach (var toInvoke in _invokeOnEvent)
            {
                var spec = toInvoke.WorkSpec;
                var postProcessor = toInvoke.PostProcessor;
                postProcessor.PostProcess(forPawn, spec, request);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref Def, "def");
            Scribe_Collections.Look(ref _invokeOnEvent, "postProcessorRefs", LookMode.Deep);
            if (_invokeOnEvent == null) _invokeOnEvent= new List<PostProcessorRef>();
            _invokeOnEvent = _invokeOnEvent.Where(x => x != null).ToList();
        }

        public static Event Create(EventDef def)
        {
            if (def == null) throw new ArgumentException("Def cannot be null");
            return new Event() { Def = def };
        }

        private class PostProcessorRef : IExposable
        {
            public WorkSpecification WorkSpec;
            public IPawnPostProcessor PostProcessor;

            public PostProcessorRef() { }

            public PostProcessorRef(WorkSpecification workSpec, IPawnPostProcessor postProcessor)
            {
                WorkSpec = workSpec;
                PostProcessor = postProcessor;
            }

            public void ExposeData()
            {
                Scribe_References.Look(ref WorkSpec, "workSpec");
                Scribe_Deep.Look(ref PostProcessor, "postProcessor");
            }
        }
    }
}
