using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public abstract class UseAbilityOnPawnPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public AbilityDef AbilityDef;
        public List<IPawnFitness> Fitness = new List<IPawnFitness>();
        public List<IPawnCondition> Conditions = new List<IPawnCondition>();

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.abilities != null && AbilityDef != null)
            {
                Ability ability = pawn.abilities.abilities.FirstOrDefault(x => x.def == AbilityDef);
                LocalTargetInfo? pawnTarget = GetTarget(workSpecification, request);
                if (pawnTarget.HasValue && pawn.jobs.curJob?.ability != ability)
                {
                    LocalTargetInfo target = pawnTarget.Value;
                    if (ability.CanApplyOn(target) && target.IsValid)
                    {
                        pawn.jobs.StartJob(ability.GetJob(target, target));
                    }
                }
            }
        }

        private LocalTargetInfo? GetTarget(WorkSpecification workSpec, ResolveWorkRequest resolveWorkRequest)
        {
            Pawn[] applicable = GetPotentialTargets(resolveWorkRequest)
                .Where(x => Conditions.All(y => y.IsValid(x, workSpec, resolveWorkRequest))).ToArray();

            PawnFitnessComparer comparer = new PawnFitnessComparer(Fitness, workSpec, resolveWorkRequest);
            Array.Sort(applicable, comparer);

            if (applicable.Length > 0)
            {
                return new LocalTargetInfo(applicable.First());
            }
            return null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref AbilityDef, "ability");
            Scribe_Collections.Look(ref Fitness, "fitness", LookMode.Deep);
            Scribe_Collections.Look(ref Conditions, "conditions", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Fitness ??= new List<IPawnFitness>();
                Conditions ??= new List<IPawnCondition>();

                Fitness = Fitness.Where(x => x.IsValidAfterLoad()).ToList();
                Conditions = Conditions.Where(x => x.IsValidAfterLoad()).ToList();
            }
        }

        public abstract IEnumerable<Pawn> GetPotentialTargets(ResolveWorkRequest resolveWorkRequest);
    }

    public class UseAbilityOnAllyPawnPostProcessor : UseAbilityOnPawnPawnPostProcessor
    {
        public override IEnumerable<Pawn> GetPotentialTargets(ResolveWorkRequest resolveWorkRequest)
        {
            return resolveWorkRequest.WorkManager.GetAllPawns();
        }
    }

    public class UseAbilityOnEnemyPawnPostProcessor : UseAbilityOnPawnPawnPostProcessor
    {
        public override IEnumerable<Pawn> GetPotentialTargets(ResolveWorkRequest resolveWorkRequest)
        {
            return resolveWorkRequest.Map.mapPawns.AllPawns.Where(x => x.HostileTo(Faction.OfPlayer));
        }
    }
}
