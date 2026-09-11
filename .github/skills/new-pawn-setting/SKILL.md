---
name: new-pawn-setting
description: Create a new pawn setting based on the request, such as a pawn fitness function, condition, or post processor. Use the ilspy-rw skill to analyze the RimWorld source code as necessary.
---

# New Pawn Setting Skill

## Workflow
- Determine what the new setting should do and classify it as a fitness function, condition, or post processor.
- Use the `ilspy-rw` skill to analyze the RimWorld source code and identify relevant classes, methods, and data structures.
- Implement the new setting in the appropriate project and location.
  - Skip this step if the setting already exists and the request is only to add it to the mod.
- Add the appropriate def XML in the `Defs` folder.
  - Def files are named by type, for example `PawnFitnessDefs` or `PawnConditionDefs`.
  - Add new features only to the latest RimWorld version folder.
- Add English translations in the `Languages` folder.
  - The base mod should only include English translations; other languages are contributed separately.
- Add or update the UI handler using `ModularPawnSettingUIHandler` and related classes.
  - If no suitable UI handler exists, create one based on the existing modular handler patterns.