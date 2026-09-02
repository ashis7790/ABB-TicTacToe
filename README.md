# Tic-Tac-Toe

A browser-based Tic-Tac-Toe application built using **Angular**, **TypeScript**, and **ASP.NET Core Web API**.

The application supports both **Player vs Player** and **Player vs Computer** modes. Game state and scoreboard state are maintained by the backend, with Angular responsible for the user interface and interaction.

---

## Project Overview

This project was developed as part of the ABB technical evaluation.

The application provides:

- 3 × 3 Tic-Tac-Toe board
- Player vs Player mode
- Player vs Computer mode
- Automatic computer moves
- Win detection
- Draw detection
- Winning-cell highlighting
- Move history
- Undo functionality
- Session-level scoreboard
- Reset Game
- Reset Scoreboard
- Backend-driven game state
- REST API integration
- Backend unit tests

---

## Technology Stack

### Frontend

- Angular
- TypeScript
- HTML
- CSS
- Angular HttpClient
- Angular Forms

### Backend

- ASP.NET Core Web API
- C#
- REST APIs
- In-memory game state

### Testing

- xUnit
- ASP.NET Core backend unit testing

### Source Control

- Git
- GitHub

---

## Application Architecture

The application follows a simple client-server architecture.

```text
+-----------------------------+
|       Angular Frontend      |
|                             |
|  - Game Board               |
|  - Game Mode                |
|  - Move History             |
|  - Scoreboard               |
|  - Game Actions             |
+-------------+---------------+
              |
              | REST API
              |
+-------------v---------------+
|      ASP.NET Core API       |
|                             |
|  Controllers                |
|  Services                   |
|  Game State                 |
|  Scoreboard                 |
|  Computer Strategy          |
+-----------------------------+
