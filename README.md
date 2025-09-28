# CoolCardGames

This repository implements a number of card games.

## Getting Started

### Build and Test

`dotnet build`

`dotnet test`

### Architecture

This uses abstract core architecture. Within `Library.csproj`, there is `Core/`, which contains
reusable classes (like `Dealer`), interfaces/abstract classes (likes `IGame`), and classes that can
be extended or left as-is (like `Card` and `Cards`).

Here is the ***planned*** architecture (we'll see how long it takes for me to lose interest):

![architecture.png](./docs/images/architecture.png)

## TODO
