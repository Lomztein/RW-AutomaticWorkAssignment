using RimWorld;

namespace Lomzie.AutomaticWorkAssignment.Defs
{
    /// <summary>Learning helpers definitions. <see href="file:///./../../1.6/Defs/ConceptsDefs.xml">Definition file</see></summary>
    [DefOf]
    public static class AWAConceptDefOf
    {
        static AWAConceptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AWAConceptDefOf));
        }

        public static ConceptDef AWA_Welcome;
        public static ConceptDef AWA_WorkManagerWindow;
        public static ConceptDef AWA_WorkManagerWindow_WorkSpecificationDetails;
    }
}
