# CoolCardGames

This repository implements a number of card games.

## Getting Started

### Architecture

This uses feature folders and abstract core architecture. Within `Library.csproj`, there is `Core/`,
which contains reusable classes (like `Dealer`), interfaces/abstract classes (likes `Game`), and
classes that can be extended or left as-is (like `Card` and `Cards`). Beyond that, there can/will be
other root directories in `Library.csproj` for other features, like account management n stuff.

From there, `Library.csproj` contains `Games/`, which has subdirectories for different game
implementations. For example, the initial game is `src/Library/Games/Hearts/`.

Since there are ambitions to having different UIs, there are also different entrypoint projects in
`src/`, starting with `src/Cli/`.

Here is the ***planned*** architecture (we'll see how long it takes for me to lose interest):

![appArchitecture.png](./docs/images/appArchitecture.png)

### High-level data flow within a game instance

Note that unbound channels are used like an event bus.

It's definitely a lil weird (at least for .NET) to use channels and threads this way, but I have
ambitions of making a cli app (and maybe also a desktop app) for this, so keeping everything in a
single binary is desired.

![dataFlow](./docs/images/dataFlow.png)

### Build and Test

1. Install .NET 10
1. `dotnet build`
1. `dotnet test`

## TODO

### Short-Term

- If game crashes, display game ID so users could report
- Now that `HeartsGame` has been more decomposed to (hopefully) improve testability, go test it and
  `HeartsSetupRound`
- How handle data visibility to diff players?
- CLI updates
    - Add a rules section to wireframe
    - Implement CLI wireframe: ![cliWireFrame.png](./docs/images/cliWireFrame.png)
      - ([docs](https://spectreconsole.net/widgets/layout))
    - Update `Driver` to be more dynamic than just hardcoding hearts stuff
    - support configuring settings, like for the game n cli itself
- Update docs and architecture diagram to better detail interactions (and setup?)
- Misc
    - Since `IPlayer` is specific to a specific card type, need to construct one per game, so could
      inject the channel into the player n take a player factory
    - revisit `HeartsGame` and `HeartsGameFactory` and see how they can be reused n cleaned up
    - Unit test `CliPlayer`
    - helpers to prompt for card(s) from many/all players?
- Actually impl `PlayerBridge`
    - Timeout requests probably
    - It'd be slick to refactor `CliPlayer`'s sync logic here (or "in" `IPlayer`)
- More unit tests!

### Other Platforms and Online Mode

- Online mode
    - REST API, maybe [grpc?](https://github.com/grpc/grpc-dotnet)
    - P2P or Server?
    - [Rollback?](https://en.wikipedia.org/wiki/Netcode#Rollback)
    - How handle reconnects? Sticky session, probably
- CLI Online Mode
- Website
- Mobile App, esp w/ offline mode too (LOL)
- Desktop App, esp w/ offline mode too (LOL)

### Other Ideas

- Have a debug mode that lets you see all the other hands and step through running each?
- Meaningful game AI
    - Would need card counting, esp to track when a player runs out of a suit
- Save mid-game
- Seeds and replayability
- Lobbies, queues, chat, notes, passwords, accounts
- Track player stats
- Sufficient logging such that games can be replayed

### Other Card Games to Implement

- Spades
- Rummy
- Chimera (altho this one isn't public domain, so prob just look into the games its based off, ichu,
  The Great Dalmuti, Big Two, and Beat the Landlord)
- Dingbat
- Bridge
- Old Maid
- Go Fish
- Pinochle
- Rummy
- Uno (altho this one isn't public domain, so prob not)
