# AGENTS.md — context for AI coding tools

## The game

Super Maze is a hybrid-casual mobile game set in a supermarket designed as a maze.

In each level, the player navigates the supermarket using a virtual joystick, collects all items from a predefined shopping list by approaching or touching them, and reaches the checkout before time runs out.

Obstacles such as customers blocking aisles, falling products, and power outages make completing each level more challenging.

Players receive 1–3 stars based on their completion time. Completing a level with 3 stars earns more coins than completing it with 2 stars, while 1 star allows progression without a coin reward.

Coins can be spent on power-ups, including extra time, automatically collecting an item at the beginning of a level, and additional lives.

The player has a shared pool of 3 lives across all levels. Failed attempts consume lives, which regenerate over time.

The game is designed for short sessions, with a 2-minute time limit per level that can be extended using power-ups. Players can complete levels faster depending on their performance. The game is played in portrait orientation using one-thumb controls.

## Tech

- Engine: Unity 6 LTS
- Programming language: C#
- Target: Mobile, portrait orientation
- Final target platforms: To be decided
- Packages and services: To be added as development progresses

## Architecture

To be defined during development.

Document the following as they are implemented:
- Game scenes
- Main gameplay systems
- Configuration and data storage
- Initialization order

## Rules for AI

- Follow the existing folder structure and naming. Don't create new top-level folders.
- Don't invent APIs. If unsure about an SDK or Unity API, say so and point to the docs.
- Don't change Player Settings, package name, API levels or build settings unless asked.
- Keep changes small and explain what you changed and why.
- Never commit secrets (keystores, passwords, API keys).
- Follow the gameplay mechanics and rules documented in this file.
- Do not introduce new gameplay mechanics or change existing ones without approval.
- Ask for clarification when game behavior or requirements are undefined.

## Conventions

To be defined by the team as development progresses.

Follow existing naming conventions, code style, and project structure once established.
