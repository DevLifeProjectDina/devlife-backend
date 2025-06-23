# devlife-backend
# DevLife Portal - Backend

This repository contains the backend source code for the DevLife Portal project, built with the .NET ecosystem.

## Technology Stack
The backend is built using the following technologies:
* **.NET 8/9 Minimal APIs**: For simplicity and faster implementation.
* **PostgreSQL**: The main database for users, scores, and game data.
* **MongoDB**: Used for storing non-relational data like user profiles and code snippets.
* **Redis**: Implemented for caching and session management.
* **SignalR**: For real-time features and WebSocket communication.

## Features
* Session-based user authentication (registration and login).
* API endpoints for all 6 mini-projects ("Code Casino", "Code Roast", etc.).
* Integration with external APIs like GitHub, Codewars, and OpenAI.
* Real-time gameplay logic for the "Bug Chase" game.
* Background analysis of GitHub repositories.

## Getting Started

### Prerequisites
* .NET 8 or 9 SDK
* Docker (for running PostgreSQL, MongoDB, and Redis easily) or local installations.

### Installation & Setup

1.  **Clone the repository:**
    ```sh
    git clone [URL_TO_YOUR_BACKEND_REPO]
    cd devlife-backend
    ```

2.  **Set up environment variables:**
    Create a `.env` file in the root directory and add the following variables:
    ```dotenv
    DATABASE_URL=postgresql://user:pass@localhost/devlife
    MONGODB_URL=mongodb://localhost:27017/devlife
    REDIS_URL=redis://localhost:6379
    GITHUB_CLIENT_ID=XXX
    GITHUB_CLIENT_SECRET=XXX
    OPENAI_API_KEY=XXX
    JWT_SECRET=your_super_secret_jwt_key
    ```
    *Update the values to match your local setup.*

3.  **Restore dependencies and run the application:**
    ```sh
    dotnet restore
    dotnet run
    ```

The API will be available at `http://localhost:5000` (or another port specified in your launch settings).
