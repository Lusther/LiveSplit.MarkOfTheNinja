# Contributing

Thanks for your interest in improving the LiveSplit.MarkOfTheNinja component!

## How to contribute

1. Fork this repository
2. Create a branch following the naming convention: `type/branch-name` (e.g. `feat/add-il-mode`, `fix/pointer-path`)
3. Make your changes
4. Test your changes in LiveSplit (see below)
5. Open a Pull Request

## Conventions

### Branches

Use the format `type/branch-name`, where type is one of: `feat`, `fix`, `refactor`, `docs`, `chore`.

### Commits

Use the format `type: description` with no body. The description must start with a verb. Examples:
- `feat: add split on scroll pickup`
- `fix: update pointer path for new build`

### Pull Requests

- All changes go through a PR, direct commits to `main` are not allowed
- PRs are merged via **squash merge** or **rebase merge**
- PR title follows the same format as commits (`type: title`) and is used as the squash commit message
- Each PR should be atomic, one logical change per PR

## Building

Create a `Directory.Build.props` next to the project (it is git-ignored) pointing to your LiveSplit folder:

```xml
<Project>
  <PropertyGroup>
    <LiveSplitPath>C:\path\to\LiveSplit</LiveSplitPath>
  </PropertyGroup>
</Project>
```

`dotnet build -c Release` then copies the DLL into `<LiveSplitPath>\Components`.

## Testing

1. Open LiveSplit and add the component to the layout
2. Launch Mark of the Ninja and check the log in `%LOCALAPPDATA%\LiveSplit.MarkOfTheNinja.log`
3. Check start (0-1), level splits, the final choice split, the 0-1 restart reset and load removal

## Supporting a new game build

Builds are identified by the `TimeDateStamp` of `game.exe` and listed in `MarkOfTheNinjaConstants.Builds`. An unknown build is logged with its timestamp and the component refuses to hook. The game is native C++, so the RVAs have to be found again for each new build.

## Reporting issues

Open an issue with the game build (shown in the `MarkOfTheNinja_Build` LiveSplit variable), the affected splits and the component log.
