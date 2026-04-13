# CoolCardGames

This repository implements a number of card games.

## Getting Started

### Build and Test

1. Install .NET 10
1. `dotnet build`
1. `dotnet test`

### Running locally

- There is a [scalar webpage](http://localhost:5222/scalar) for the web API.
- There is a [swagger webpage](http://localhost:5222/swagger/index.html) for the web API too.

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

## TODO

### Short-Term

- Web API (so can vibe code frontend; test what heard about good prototype but bad at iterating)
    - replace `ok(null)`s w/ 410s
    - `WebPlayer`: there are still concurrency bugs in this class
        - on the second trick, "The game never reviewed the cards before timing out"
        - esp around ifNotNullSelectCardFollowingTheseRules not being updated after the first round
        - prob want a state machine
        - prob want docs once finished
    - `GameSessionController`
        - this assumes sticky sessions (so new events can be passed)
        - use jwt in session selection logic
        - if they PlayCard when need to PlayCards, don't time out
        - how clean up finished sessions (esp on exc)?
        - in resp, don't have Hand w/ cards, have Card w/ Location enum?
        - finish separating library and api types
    - openapi serialize enums as strings
    - Use a hardcoded jwt, for now
    - More things to iteratively build. This way, can see how well AI behaves when given updates
        - General web API quality (POST vs PUT, problem details, etc)
        - Timeouts (player bridge)
        - Real auth
        - Configure user/game settings
        - Pagination of gameInstances
        - Multiplayer (posting a game session, requiring a password like DS3, etc)
        - [Rollback](https://en.wikipedia.org/wiki/Netcode#Rollback)
    - How handle data visibility to diff players? Then add to `GameSessionGetCurrentStateResponse`
        - Might wanna pass GetCards() or GetVisibleState() delegate to player
- More unit tests!
    - `IReadOnlyListExtensions.FindIndex`
    - `Game.Play` and `Game.PlayAndDisposeInBackgroundThread`
    - `ChannelGameEventPublisher`
    - Most of Hearts
    - CLI
- Merging `PlayCard` and `PlayCards` to remove duplication
- `GameRegistry`'s `MetaData` maybe possibly could use some more work around containing factories
    - What about setting overrides?
- CLI updates
    - If game crashes, display game ID so users could report
    - Add a rules section to wireframe
    - Implement CLI wireframe: ![cliWireFrame.png](./docs/images/cliWireFrame.png)
        - ([docs](https://spectreconsole.net/widgets/layout))
    - Update `Driver` to be more dynamic than just hardcoding hearts stuff
    - support configuring settings, like for the game n cli itself
- Update docs and architecture diagram to better detail interactions (and setup?)
- Misc
    - revisit `HeartsGame` and `HeartsGameFactory` and see how they can be reused n cleaned up
    - Unit test `CliPlayer`
    - helpers to prompt for card(s) from many/all players?
- Actually impl `PlayerBridge`
    - Timeout requests probably
    - If playing by themselves, have no timeout. Otherwise, 30 sec
    - It'd be slick to refactor `CliPlayer`'s sync logic here (or "in" `IPlayer`)
- Implement durability of games (redis, prob)

### Other Platforms and Online Mode

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
