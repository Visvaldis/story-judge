# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StoryJudge is a behavioral interview story review platform where users create STAR-format stories, share them for peer review with rubric-based scoring, and track coverage of key interview signals.

## Technology Stack

- **Frontend**: Vue 3 + TypeScript + Vite + Pinia
- **Backend**: .NET 10 (C#) Web API
- **Database**: MongoDB
- **Deployment**: Google Cloud Run with GitHub Actions CI/CD

## Quick Start

```bash
# Start all services (MongoDB, Backend, Frontend)
./start.sh

# Or manually:
cd backend && docker-compose up -d          # Start MongoDB
cd backend && dotnet run --project src/StoryJudge.Api
cd frontend && npm run dev
```

**URLs:**
- Frontend: http://localhost:5173
- Backend API: http://localhost:5107/api
- Swagger: http://localhost:5107/swagger (dev only)

## Build Commands

**Backend:**
```bash
cd backend
dotnet restore
dotnet build
dotnet test                                    # Run all tests
dotnet run --project src/StoryJudge.Api       # Run API
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev          # Development server
npm run build        # Production build
npm run lint         # ESLint check
npm run lint:fix     # Auto-fix lint issues
```

## Architecture

### Backend Structure (Clean Architecture)
```
backend/src/
├── StoryJudge.Api/           # ASP.NET Core Web API
│   ├── Controllers/          # REST endpoints
│   ├── Middleware/           # Error handling, etc.
│   └── Program.cs            # DI setup, middleware pipeline
├── StoryJudge.Core/          # Domain layer
│   ├── Models/               # Domain entities (User, Story, Review, Report)
│   ├── DTOs/                 # Data transfer objects
│   ├── Enums/                # Visibility, StoryType, RoleLevel, etc.
│   ├── Services/             # Business logic
│   └── Interfaces/           # Repository & service contracts
├── StoryJudge.Infrastructure/ # Data access
│   ├── Data/                 # MongoDbContext
│   └── Repositories/         # MongoDB implementations
└── StoryJudge.Tests/         # Unit tests
```

### Frontend Structure
```
frontend/src/
├── views/           # Page components (Dashboard, StoryEditor, Explore, etc.)
├── components/      # Reusable components (Navbar, ReviewForm, StoryCard)
├── stores/          # Pinia stores (auth, story, review)
├── services/        # API client (api.ts, auth.ts, stories.ts, reviews.ts)
├── router/          # Vue Router configuration
└── types/           # TypeScript interfaces
```

### Key Patterns

- **Authentication**: JWT tokens + OAuth (Google/LinkedIn). Auth state managed in Pinia `auth` store.
- **API Client**: Axios instance with token interceptor in `frontend/src/services/api.ts`
- **Story Visibility**: Three-tier system (PRIVATE/UNLISTED/PUBLIC) with shareToken for unlisted
- **Review Aggregation**: Denormalized scores stored on Story entity for performance

## Configuration

Backend config is in `backend/src/StoryJudge.Api/appsettings.json`. Key settings:
- `MongoDB:ConnectionString` - Database connection
- `Jwt:Key` - JWT signing key (use secrets in production)
- `Cors:AllowedOrigins` - Allowed frontend origins

Production overrides via Cloud Run environment variables and secrets.

## CI/CD

GitHub Actions workflow in `.github/workflows/ci-cd.yml`:
- **CI**: Build + test backend, lint + build frontend
- **CD**: Deploy to Google Cloud Run (triggers on push to master)

Required GitHub secrets: `GCP_PROJECT_ID`, `GCP_SA_KEY`, MongoDB and OAuth secrets in Secret Manager.
