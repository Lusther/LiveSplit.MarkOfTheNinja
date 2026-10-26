# LiveSplit.MarkOfTheNinja

A [LiveSplit](https://livesplit.org/) component autosplitter and load remover for [Mark of the Ninja](https://store.steampowered.com/app/214560/Mark_of_the_Ninja/) speedruns (original release, not the Remastered edition).

## Features

- Auto-start when the player gets control in 0-1
- Split at the end of each level
- Split on the final choice input in 4-2 (kill Azai or yourself), timed to the input itself
- Auto-reset when 0-1 is restarted from the pause menu
- Load removal: only the loading screen is removed, the shop, loadout and intro videos still count

Every option can be turned off in the component settings. For load removal, compare against **Game Time** (right-click > Compare Against > Game Time).

## Installation

1. Download the latest release
2. Place the component DLL in your LiveSplit `Components` folder
3. In LiveSplit, right-click > Edit Layout > Add > Control > **Mark of the Ninja Autosplitter**

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

[MIT](LICENSE)
