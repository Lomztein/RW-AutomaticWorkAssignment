using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Dedications : IExposable
    {
        private List<Dedication> _dedications = new List<Dedication>();

        public void Dedicate(Pawn pawn, WorkSpecification toWork, int expirationTick)
        {
            Dedication current = _dedications.FirstOrDefault(x => x.WorkSpec == toWork && x.Pawn == pawn);
            if (current == null)
            {
                Dedication newDedication = new Dedication(toWork, pawn, expirationTick);
                _dedications.Add(newDedication);
            }
        }

        public IEnumerable<Pawn> GetDedicatedPawns(WorkSpecification workSpec)
        {
            _dedications.RemoveAll(x => x.Pawn == null || x.Pawn.Dead || x.IsExpired());
            IEnumerable<Dedication> dedications = _dedications.Where(x => x.WorkSpec == workSpec);

            if (dedications.Any())
            {
                return dedications.Select(x => x.Pawn);
            }

            return Enumerable.Empty<Pawn>();
        }

        public void ClearDedications()
            => _dedications.Clear();

        public void ClearDedications(WorkSpecification workSpecification)
        {
            _dedications.RemoveAll(x => x.WorkSpec == workSpecification);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _dedications, "dedications", LookMode.Deep);
            _dedications ??= new List<Dedication>();
        }

        public class Dedication : IExposable
        {
            public WorkSpecification WorkSpec;
            public PawnRef PawnRef;
            private int _expirationTick;

            public void ExposeData()
            {
                Scribe_References.Look(ref WorkSpec, "workSpec");
                Scribe_Deep.Look(ref PawnRef, "pawn");
                Scribe_Values.Look(ref _expirationTick, "experiationTick");
            }

            public Dedication(WorkSpecification workSpec, Pawn pawn, int expirationTick)
            {
                WorkSpec = workSpec;
                PawnRef = new PawnRef(pawn);
                _expirationTick = expirationTick;
            }

            public Dedication() { }

            public Pawn Pawn
                => PawnRef?.Pawn;

            public bool IsExpired()
                => GenTicks.TicksAbs >= _expirationTick;
        }
    }
}
