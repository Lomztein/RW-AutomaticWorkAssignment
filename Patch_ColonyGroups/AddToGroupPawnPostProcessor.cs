using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Linq;
using TacticalGroups;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.ColonyGroups
{
    public class AddToGroupPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public PawnGroup Group;
        private string _groupName;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (pawn == null)
                return;

            var groups = TacticUtils.TacticalGroups?.pawnGroups;
            if (groups == null)
                return;

            if (Group == null && _groupName != null)
            {
                var matches = groups.Where(x => x.curGroupName == _groupName).ToArray();
                if (matches.Length == 1)
                    Group = matches[0];
            }

            if (Group != null && groups.Contains(Group))
            {
                Group.autoDisbandWithoutPawns = false;
                Group.Add(pawn);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Group, "group");
            if (Scribe.mode == LoadSaveMode.Saving)
                _groupName = Group?.curGroupName;
            Scribe_Values.Look(ref _groupName, "groupName");
        }
    }
}
