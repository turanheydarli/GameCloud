 # GameCloud Backend System

GameCloud is a modular backend platform designed to support turn-based asynchronous multiplayer games. It enables game developers to easily create, manage and scale their games by providing a secure and flexible architecture for executing custom game logic, managing player data, and handling state transitions.

## Purpose

The primary goal of GameCloud is to empower game developers with the tools they need to build dynamic and engaging multiplayer experiences without having to worry about the complexities of backend infrastructure, state management, and security. By leveraging cloud-hosted functions and well-defined entities, developers can focus on creating unique game mechanics and player interactions.

## Key Features

- **Modular Function Architecture**: GameCloud allows developers to register and execute custom functions for handling game logic. These functions can be hosted on various cloud platforms like AWS Lambda, Cloudflare Workers, etc. Strict input/output validation ensures data consistency and prevents exploitation.

- **Comprehensive Entity Model**: The backend provides a set of core entities and relationships for managing games, sessions, players, and game states:
  - `Developer`: Manages games, configurations, and cloud integrations. 
  - `Game`: Contains metadata, rules, limits, and references associated projects.
  - `Session`: Represents active matches with participants and current state.
  - `Player`: Manages player roles, scores, and interactions within a session.
  - `User`: Represents a platform-wide user with notification subscriptions.

- **Fraud Prevention and Fairness**: GameCloud enforces strict validation of player actions to prevent cheating. All state mutations occur server-side based on the outcome of custom functions. Developers can configure rate limits and cooldowns.

- **Scalability and Portability**: By leveraging serverless cloud functions, GameCloud can automatically scale to handle large player bases and match volumes. Games can be easily deployed across different cloud platforms.

## Getting Started

To begin using GameCloud for your game, follow these steps:

1. Register for a GameCloud developer account at [cloud.playables.studio](https://cloud.playables.studio).
2. Create a new game project and configure your preferred cloud provider.
3. Define your game rules, session flow, and custom function endpoints. 
4. Integrate the GameCloud client SDK into your game app.
5. Deploy your custom functions and start running matches!

For more details and API references, check out the [official documentation](https://docs.cloud.playables.studio).

## License

GameCloud is available under the [MIT License](LICENSE).