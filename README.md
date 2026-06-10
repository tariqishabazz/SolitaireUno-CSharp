[![.NET Core Check](https://github.com/tariqishabazz/SolitaireUno-Refactored-/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tariqishabazz/SolitaireUno-Refactored-/actions/workflows/dotnet.yml)

# Solitaire-Uno (C# / .NET 8)

A decoupled, unit-tested implementation of a hybrid Solitaire-Uno game engine. This project focuses on **Clean Architecture**, **Dependency Inversion**, and **Automated Testing**.

## Key Engineering Features
*   **Interface-Driven Design:** Utilizes `IInputProvider` and `IOutputProvider` to decouple game logic from the Console, allowing for future GUI or Web API integration.
*   **Object-Oriented Architecture:** Implements specialized card types (`RegularCard`, `SpecialCard`) using inheritance and polymorphism.
*   **State Management:** Decoupled turn handling through `PlayerTurnHandler` and `ComputerTurnHandler`.
*   **CI/CD Pipeline:** Automated build and test workflows via **GitHub Actions**.

## 🛠️ Tech Stack
*   **Language:** C# 12 / .NET 8
*   **Testing:** xUnit
*   **Automation:** GitHub Actions

## Testing Strategy
The project includes a dedicated `SolitaireUno.Tests` suite. I focused on testing "Business Logic" rather than UI, specifically:
*   Game state transitions after special card plays.
*   Validation of legal moves based on color/value matching.
*   Computer AI decision-making logic.

## Known Issues

