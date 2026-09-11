---
name: ilspy-rw
description: This can be used to decompile and analyze RimWorld source code.
---

To decompile and analyze RimWorld source code, follow these steps.
Use a TODO tool if available to keep track of unexplored notes, as well as provide the user with a progress overview.

1. Invoke the ilspycmd .NET tool to decompile and inspect RimWorld assembly (DLL) files. Use the search functionality to locate the specific class or method you are interested in analyzing.
2. Common ilspycmd syntax:
   - `ilspycmd <assembly.dll>` decompiles to console output.
   - `ilspycmd -o <output-folder> <assembly.dll>` writes decompiled output to a directory.
   - `ilspycmd -p -o <output-folder> <assembly.dll>` creates a compilable project.
   - `ilspycmd --nested-directories -p -o <output-folder> <assembly.dll>` creates a project with nested namespace folders.
   - `ilspycmd <assembly.dll> --list-resources` lists embedded resources.
   - `ilspycmd <assembly.dll> --resource <resource-name> -o <output-folder>` extracts a specific resource.
   - `ilspycmd <assembly.dll> --generate-diagrammer` creates an HTML diagrammer.
3. If the tool is unavailable, instruct the user to install it using the `dotnet tool install --global ilspycmd` command.

RimWorld assemblies can commonly be found in the following locations:
	C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll

If the assemblies are not found, check if they are specified in the Directory.Build.props file.
If assemblies remain unable to be located, abort the analysis and inform the user that the assemblies could not be found.

4. Analyze and summarize the output. Take note of other relevant classes to explore, but keep the scope limited.
5. Continue analysis until you have a good understanding of the requested source code.

Summarize your findings to the user, with emphasis on how to interact with the code.