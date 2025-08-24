# Adding user lessons

Lessons are also known as `Concept`/`PlayerKnowledge`/`Lesson` in RW codebase.

## XML

You can declare concepts with `<ConceptDef>` XML nodes. Example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
  <ConceptDef> <!-- This is a real sample for the `TimeControls` concept -->
    <defName>MyMod_NoOpportunity</defName>
    <label>Time controls</label>
    <priority>93</priority>
    <helpText>You can speed up time.\n\nTry controlling time with the {Key:TimeSpeed_Normal}, {Key:TimeSpeed_Fast}, and {Key:TimeSpeed_Superfast} keys, or with the time controls in the bottom right.</helpText>
    <helpTextController>You can speed up time.\n\nTry controlling time with {Key:TimeSpeed_Faster} and {Key:TimeSpeed_Slower}, or with the time controls in the bottom right.</helpTextController>
    <highlightTags>
      <li>TimeControls</li>
    </highlightTags>
  </ConceptDef>
  <ConceptDef> <!-- This is a real sample for the `ReformCaravan` concept -->
    <defName>MyMod_Opportunity</defName>
    <label>Reforming caravans</label>
    <priority>50</priority>
    <needsOpportunity>True</needsOpportunity>
    <helpText>After an encounter, you can easily re-form a caravan by using the RE-FORM CARAVAN command in the World view.</helpText>
    <highlightTags>
      <li>MainTab-World-Closed</li>
      <li>ReformCaravan</li>
    </highlightTags>
  </ConceptDef>
</Defs>
```

See basic schema: https://rimworldwiki.com/wiki/User:Alistaire/Tag:ConceptDef

### Fields details:

- **needsOpportunity**: if `true`, the concept will be displayed in the learning helper only when imperatively requested, using `LessonAutoActivator.TeachOpportunity(ConceptDef conc, OpportunityType opp)`
- **helpText**: Supports:
    - [limited Unity UI Rich Text](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/StyledText.html) (at least `<b>`, `<i>`, `<size>`, `<color>`, but I did not managed to make `<quad>` work). Tags must be HTML-entity-encoded (eg . `&lt;b&gt;Example&lt;/b&gt;`)
        > [!NOTE]
        > RimWorld codebase use IMGUI under the hood, but I did not find a clear list of tags supported by its styled text. If you have better refs, please send them.
    - `{Key:<keyName>}` placeholders, to display actual user-defined key bindings
    - A few style aliases (in enum `TagType`), for example `(*SectionTitle)Hello(/SectionTitle)`
        > [!NOTE]
        > I found references to such tags in the code, yet I'm not sure they work properly.
- **highlightTags**: A list of GUI elements to highlight. They can be arbitrary (defined directly in the code, see bellow) or conventional:
    - Highlight bottom `MainButtonDef` with `MainTab-<MainButtonDef.defName>-Closed`

## Load them

You'll very likely need to reference them somewhere in your code, so load them:

```cs
using RimWorld;

namespace MyMod.Defs
{
    [DefOf]
    public static class MyConceptDefOf
    {
        static MyConceptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MyConceptDefOf));
        }

        public static ConceptDef MyMod_NoOpportunity;
        public static ConceptDef MyMod_Opportunity;
    }
}
```

## Use lessons programmatically

### Highlight some parts of the interface

```cs
UIHighlighter.HighlightOpportunity(rect, highlightTag_li);
```

### Mark a concept as learned

```cs
PlayerKnowledgeDatabase.KnowledgeDemonstrated(MyConceptDefOf.MyMod_NoOpportunity, KnowledgeAmount.Total);
```

### Start an opportunity to learn something

```cs
LessonAutoActivator.TeachOpportunity(MyConceptDefOf.MyMod_Opportunity, OpportunityType.GoodToKnow);
```
