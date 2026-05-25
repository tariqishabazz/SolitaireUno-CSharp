[![.NET Core Check](https://github.com/tariqishabazz/SolitaireUno-Refactored-/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tariqishabazz/SolitaireUno-Refactored-/actions/workflows/dotnet.yml)

# Solitaire-Uno (C# / .NET 8)

A decoupled, unit-tested implementation of a hybrid Solitaire-Uno game engine. This project focuses on **Clean Architecture** and **Automated Testing**.

## Key Engineering Features
*   **GUI-Driven Design:** Utilizes Blazor to create a GUI for enhanced gameplay.
*   **Object-Oriented Architecture:** Implements specialized card types (`RegularCard`, `SpecialCard`) using inheritance and polymorphism.
*   **State Management:** Decoupled turn handling through `PlayerTurnHandler` and `ComputerTurnHandler`.
*   **CI/CD Pipeline:** Automated build and test workflows via **GitHub Actions**.

## 🛠️ Tech Stack
*   **Language:** C# 12 / .NET 8
*   **Testing:** xUnit
*   **Automation:** GitHub Actions

## Testing Strategy
The project includes a dedicated `SolitaireUno.Tests` suite. I focused on testing "Business Logic" rather than UI (coming soon), specifically:
*   Game state transitions after special card plays.
*   Validation of legal moves based on color/value matching.
*   Computer AI decision-making logic.

## Known Issues


## How to Run
1. Clone the repo: `git clone https://github.com`
2. Navigate to the App folder: `cd SolitaireUno`
3. Run: `dotnet run`
