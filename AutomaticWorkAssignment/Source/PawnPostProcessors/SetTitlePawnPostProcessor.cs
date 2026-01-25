using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetTitlePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public string Title = string.Empty;
        public bool AllowOverwrite = true;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            bool setTitle = !request.GetVariable<bool>(pawn.GetUniqueLoadID() + "_TitleSet");
            if (setTitle)
            {
                string title = string.IsNullOrEmpty(Title) ? pawn.story.TitleDefaultCap : Title;
                pawn.story.title = title;

                if (!AllowOverwrite)
                    request.SetVariable(pawn.GetUniqueLoadID() + "_TitleSet", true);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Title, "title", defaultValue: string.Empty);
            Scribe_Values.Look(ref AllowOverwrite, "allowOverride", defaultValue: true);
        }
    }
}
