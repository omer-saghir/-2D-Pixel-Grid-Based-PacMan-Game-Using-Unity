# 2D Pixel Grid Based PacMan Game

This is a dynamic, fast-paced action game developed in **Unity** using **C#**. It evolves the classic "Pac-Man" concept into a survival-style dungeon crawler. The game features an escalating difficulty system where new enemy variants unlock as the player scores points, culminating in a high-stakes final boss battle.

# Features

### Player Mechanics & Power-ups
* **Dynamic Movement:** Smooth, grid-aligned horizontal movement with automatic screen-wrapping (teleporting from one edge to the other).
* **Power-Up System:** Collecting "Power" items transforms the player, increasing movement speed and changing the character skin.
* **Magnet Effect:** While powered up or during boss fights, the player gains a "Magnet" ability that pulls all nearby food items toward them.
* **Combat System:** In power-up mode, players can shoot **Cyan Fireballs** to destroy enemies and damage the final boss.

### Enemy & Level Progression
* **Escalating Difficulty:** New enemy variants (Red, Green, Octopus, Death, etc.) are automatically unlocked from a queue every 30 points.
* **Boss Battle System:** Upon reaching 180 points, the game transitions into a Boss Phase, spawning a "Final Boss" with a dedicated health bar and unique red fireball attacks.
* **AI & Movement:** Enemies utilize a custom `InternalMover` system to navigate the screen at varying speeds.

### Systems & UI
* **Score Management:** A centralized `ScoreManager` tracks points and triggers level milestones and boss arrivals.
* **Custom Software Cursor:** Implements a specialized UI-based cursor that replaces the standard OS mouse for a consistent pixel-art aesthetic.
* **Audio Integration:** Features a dual-track music system that switches between standard exploration music and high-energy power-up themes.
* **Robust UI:** Includes a dynamic Boss HP slider, real-time score tracking, and an interactive Main Menu with scene management.

# Technical Stack

* **Game Engine:** Unity
* **Language:** C# (Object-Oriented Programming)
* **UI Framework:** TextMeshPro & Unity UI
* **Design Patterns:** Singleton (ScoreManager), Component-Based Architecture

# Prerequisites
* **Unity Hub & Unity Editor** 
* **Visual Studio** for C# script editing
* **Assets:** Requires sprites for various ghost variants, food items, and the boss prefab as defined in the `Player.cs` inspector slots.

* **Unity Hub & Unity Editor** (Recommended 2021.3 LTS or newer)
* **Visual Studio** for C# script editing
* **Assets:** Requires sprites for various ghost variants, food items, and the boss prefab as defined in the `Player.cs` inspector slots.
