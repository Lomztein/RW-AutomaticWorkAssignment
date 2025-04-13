using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class PawnWorkPriorities : IExposable
    {
        public List<WorkTypeDef> OrderedPriorities;

        public static PawnWorkPriorities CreateDefault()
        {
            var priorities = new PawnWorkPriorities();

            priorities.OrderedPriorities = new List<WorkTypeDef>();
            foreach (var def in DefDatabase<WorkTypeDef>.AllDefs)
            {
                priorities.OrderedPriorities.Add(def);
            }
            return priorities;
        }

        internal static PawnWorkPriorities CreateEmpty()
        {
            var priorities = new PawnWorkPriorities();
            priorities.OrderedPriorities = new List<WorkTypeDef>();
            return priorities;
        }

        public void RemovePriority(WorkTypeDef priority)
        {
            Find.Root.StartCoroutine(DelayedRemovePriority(priority));
        }

        private IEnumerator DelayedRemovePriority(WorkTypeDef priority)
        {
            yield return new WaitForEndOfFrame();
            OrderedPriorities.Remove(priority);
        }

        public void MovePriority(WorkTypeDef priority, int movement)
        {
            Find.Root.StartCoroutine (DelayedMovePriority(priority, movement));
        }

        private IEnumerator DelayedMovePriority(WorkTypeDef priority, int movement)
        {
            yield return new WaitForEndOfFrame();
            Utils.MoveElement(OrderedPriorities, priority, movement);
        }

        public void AddPriority(WorkTypeDef newPriority)
        {
            Find.Root.StartCoroutine(DelayedAddPriority(newPriority));
        }

        private IEnumerator DelayedAddPriority(WorkTypeDef priority)
        {
            yield return new WaitForEndOfFrame();
            OrderedPriorities.Add(priority);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref OrderedPriorities, "orderedPriorities", LookMode.Def);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (OrderedPriorities == null) OrderedPriorities = new List<WorkTypeDef>();
            }
        }
    }
}
