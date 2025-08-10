## Setup your environment

### 1. Configure RimWorld Steam directory

> [!NOTE]
> Required only if RimWorld isn't installed at the default path (`C:\Program Files (x86)\Steam\steamapps`)

Create a new `Directory.Build.props.user` file at the root of the repository. You can use [Directory.Build.props.user.example](Directory.Build.props.user.example) as an example

### 2. Install dependencies

You will need [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) installed.

In addition, you can install any/all the optional dependency mods you want to work with from [./AutomaticWorkAssignment/LoadFolders.xml](./AutomaticWorkAssignment/LoadFolders.xml)

## Dev process

### Formatting

Think about formatting your code with [./format.bat](./format.bat) regularly.

```sh
./format.bat
```

In addition to this, you can run auto cleanups in Visual Studio with "Analyze" (in the very top of VS) > "Code Cleanup" > "Run Code Cleanup (Profile 1) on Solution". This can be configured to run automatically as described in [this stackoverflow response](https://stackoverflow.com/a/71597587)
