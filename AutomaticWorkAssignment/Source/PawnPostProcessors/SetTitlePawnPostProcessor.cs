using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetTitlePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public string Title = string.Empty;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            string title = string.IsNullOrEmpty(Title) ? pawn.story.TitleDefaultCap : Title;
            pawn.story.title = title;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Title, "title", defaultValue: string.Empty);
        }
    }
}
