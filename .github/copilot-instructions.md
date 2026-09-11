# Copilot Instructions

## Repository Overview
- This repository contains RimWorld mods and patches for AutomaticWorkAssignment.
- The core project is `AutomaticWorkAssignment`, which contains the main mod logic, framework, data structures, ect.
- Larger mod compatability pathes are contained in the `AutomaticWorkAssignment.Patches` project.
- First-party addons are contained in the `AutomaticWorkAssignment.Addons` project.
- The solution targets .NET Framework 4.8 projects; follow the target of the specific project you are editing.

## Design Principles
- Prefer event-triggered orchestration for gameplay behavior.
- Make use of caching where appropriate to avoid unnecessary recalculation of assignments, in particular per-frame.
- Main-mod assignments are recalculated at regular intervals and should not rely on persistent state between assignment cycles.
- Keep changes focused on the existing mod architecture rather than introducing new frameworks or patterns unless required.
- Readability, expandability, and maintainability are the key qualities.

## RimWorld-Specific Guidance
- Use the `ilspy-rw` skill to inspect RimWorld source behavior when necessary and if available.
- Use the `new-pawn-setting` skill when adding pawn settings, fitness functions, conditions, or post processors.
- Favor compatibility with existing RimWorld systems and mod interoperability.

## Key Systems and Patterns
- Central component is 'MapWorkManager', which acts as a central controller for work assignment orchestration.
- Work is specified in a 'WorkSpecification', which contains general data as well as a fitness, conditions, and post processors.
- Assignments are made as a 'WorkAssignment', these are ephemeral and recalculated at regular intervals.
- IPawnSetting is the root interface for pawn settings, with PawnSetting as the default implementation.
- IPawnFitness, IPawnCondition, and IPawnPostProcessor define different kinds of settings. A new setting must implement one of these.
- The Settings class handles the variables and UI logic for the mod's global settings.

## Key Utilities
- IO provides functionality for loading, saving, and serializing data.
- Buffer provides a general-purpose buffer for temporary data storage and manipulation.
- Cache and CacheDict provide caching functionality for data that is expensive to calculate or retrieve per frame.
- Clipboard provides utilities for deep copying and pasting of IExposable objects.

## Code and Project Guidance
- Match the style, namespaces, and patterns already used in the target project.
- Keep changes minimal and localized to the relevant addon or patch project.
- Update tests when behavior changes affect automated coverage.

## Validation
- Build the affected project or solution after changes.
- Run relevant tests for behavior changes when test coverage exists.
- Investigate compilation or runtime issues at the source rather than masking them.
