# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StoryJudge is a behavioral interview story review platform where users create STAR-format stories, share them for peer review with rubric-based scoring, and track coverage of key interview signals.

## Technology Stack

- **Frontend**: Vue.js
- **Backend**: C# .NET
- **Database**: MongoDB
- **Deployment**: GCP

## Project Status

This project is in the pre-implementation phase. The design document (`README.md`) contains the complete specification including data models, API endpoints, and implementation plan.

## Expected Project Structure

When implementing, organize the codebase as:
```
/backend          # C# .NET API
/frontend         # Vue.js application
```

## Build Commands (once implemented)

**Backend (.NET):**
```bash
dotnet restore
dotnet build
dotnet run --project backend
dotnet test
```

**Frontend (Vue.js):**
```bash
cd frontend && npm install
cd frontend && npm run dev
cd frontend && npm run build
cd frontend && npm run lint
```

## Key Architecture Decisions

### Data Model
Core entities: User, Story, Review, StoryCoverage, Report, Reaction. See design doc section 7 for full schema.

### Story Visibility
Three-tier system requiring careful permission checks:
- **PRIVATE**: Author only
- **UNLISTED**: Author + anyone with shareToken
- **PUBLIC**: All authenticated users

### Review System
7-category rubric scoring (1-5 scale): Clarity, Ownership, Impact, Decision Making, Communication, Reflection, Technical Depth (optional). Store denormalized aggregates on Story for performance.

### Coverage Detection
MVP: Manual checklist + basic regex heuristics (detect metrics, "I" language, non-empty sections). V1: AI-assisted detection.

## Implementation Order

Follow the 8-phase plan from design doc section 18:
1. Backend setup + auth + database schema
2. Story CRUD + visibility handling
3. Explore feed (public stories)
4. Review system + aggregation
5. Coverage checklist + heuristics
6. Dashboard + story analytics
7. Reporting + admin tools
8. Rate limiting + polish
