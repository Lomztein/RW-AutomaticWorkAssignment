---
name: mod-compatibility-patch
description: Create compatibility patches for third-party RimWorld mods. Ask for the other mod's assembly path and use the ilspy-rw skill to analyze it before implementing the patch.
---

# Mod Compatibility Patch Skill

## Workflow
- Ask the user for the path to the other mod's assembly file if it is not already known.
- Use the `ilspy-rw` skill to analyze the other mod's assembly and identify the classes, methods, and data flow that need to be patched.
- Determine the most appropriate patch project in this repository, usually a `Patch_` project that matches the target mod.
-	If one does not already exist, ask the user if one should be created.
- Implement the compatibility patch with the smallest change that achieves the desired behavior. Use Harmony as needed, but prioritize simplicity and maintainability.
- Keep the patch aligned with the existing project style and RimWorld mod conventions.
- Add or update XML defs, Harmony patches, or supporting code only when needed for the compatibility behavior.
- Validate the patch by building the affected project and reviewing any relevant tests or runtime assumptions.

## Guidance
- Prefer compatibility over invasive redesign.
- Limit changes to the patch project unless the request clearly requires shared infrastructure changes.
- If the other mod's assembly cannot be located or analyzed, stop and inform the user before making assumptions.
- Summarize the compatibility approach clearly, including what was patched and why.
