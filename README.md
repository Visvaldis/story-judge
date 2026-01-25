# "StoryJudge" (Behavioral Interview Story Review Platform)

## Quick Start

```bash
# Start all services (MongoDB, Backend, Frontend)
./start.sh

# Stop all services
./stop.sh
```

Once running:
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

### Prerequisites
- Docker (for MongoDB)
- .NET 10 SDK
- Node.js 18+

### OAuth Setup (Optional)
To enable Google/LinkedIn login, add your OAuth credentials to `backend/src/StoryJudge.Api/appsettings.json`:
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "LinkedIn": {
      "ClientId": "your-linkedin-client-id",
      "ClientSecret": "your-linkedin-client-secret"
    }
  }
}
```

---

## 1) Purpose & Vision

### Problem

Candidates write behavioral interview stories, but:

* they don’t know if the story is “strong” vs “sounds weak”
* they miss key signals (ownership, impact, conflict handling, metrics)
* they can’t easily iterate with feedback cycles
* they don’t have a structured bank + tagging system for interview prep

### Solution

A web application where a user can:

1. create and store stories in a structured format
2. optionally publish stories to be reviewed by others (community)
3. receive ratings + comments by rubric categories
4. see “coverage” of key story components (STAR + common hiring signals)
5. iterate with versions and track improvements over time

---

## 2) Scope

### MVP (must ship first)

* Auth + user profiles
* Story creation (rich text + structured fields)
* Story privacy: **Private / Unlisted link / Public**
* Community feed to discover public stories
* Review system: rubric rating + comment
* “Coverage checklist” (manual) + simple auto checks (non-AI rules)
* Story dashboard: scores over time + what’s missing
* Basic moderation/reporting

### V1 / Next phase

* AI-assisted “coverage detection” and rewrite suggestions (optional)
* Story templates by question type (failure, conflict, leadership, ambiguity)
* Versioning + diff + “before/after” comparison
* Collections (story packs for interview loop)
* Notifications (when you get reviews)
* Trust / reputation system for reviewers

### Non-goals (for now)

* Real-time collaborative editing
* Video/audio stories
* Paid coaching marketplace
* Mobile native apps (web only initially)

---

## 3) Key User Personas & Jobs

### Candidate (primary)

* “I need strong stories for behavior rounds”
* Wants structure + storage + quick iteration + honest scoring

### Reviewer (secondary)

* “I can help others and practice evaluation skills”
* Wants quick review UI + lightweight scoring + visibility

### Moderator/Admin

* Remove spam, hate, plagiarism
* Keep community safe and useful

---

## 4) Core Concepts & Definitions

### Story

A behavioral interview story about a work situation. Stored as:

* title
* context/problem
* actions you took
* results/impact
* reflections/lessons
* metadata tags (competencies, role level, domain)

### Rubric (for reviews)

Standardized scoring categories (1–5 scale) to normalize feedback:

* Clarity / Structure
* Ownership
* Impact (metrics/results)
* Decision making / Tradeoffs
* Communication / Stakeholder mgmt
* Technical depth (optional)
* Reflection / Learning

### Coverage (what points you covered)

A story quality checklist (boolean items) such as:

* Has situation context
* Has clear goal/problem statement
* Mentions constraints
* Shows your actions specifically (not “we”)
* Includes measurable impact
* Mentions conflict/ambiguity handling (if relevant)
* Mentions learnings
* Mentions what you’d do differently

Coverage should be visible to the author as “missing signals”.

---

## 5) UX / Main User Flows

### 5.1 Create Story

1. User clicks “New Story”
2. Selects template (optional): “Conflict”, “Failure”, “Leadership”, etc.
3. Fills structured fields (STAR + Reflection)
4. Adds tags (skills, company type, level, question type)
5. Sets visibility: Private / Unlisted / Public
6. Saves

### 5.2 Request Reviews

* If story is Public: it appears in feed
* If Unlisted: user shares a link
* Reviewers can rate + comment

### 5.3 Improve Story

* User sees:

  * average score per rubric category
  * coverage checklist (missing items)
  * common feedback themes
* User edits story and saves as **new version** (optional for MVP, recommended for V1)

### 5.4 Reviewer Flow

1. Opens story
2. Reads quick structured summary (Situation/Task/Action/Result)
3. Rates rubric
4. Adds comment with suggestions
5. Submits

### 5.5 Moderation Flow

* Report story or review
* Admin dashboard:

  * list of reports
  * hide content
  * ban user / shadowban
  * audit logs

---

## 6) Requirements

### Functional Requirements

* Users can sign up/login
* Users can create/edit/delete their stories
* Stories can be private/unlisted/public
* Public stories are browseable + searchable
* Any logged-in user can review public/unlisted stories
* Reviews contain:

  * numeric scores per rubric category
  * short written feedback
* Author sees review aggregation + coverage
* Users can report abusive content
* System supports basic rate limits + spam protection

### Non-Functional Requirements

* Fast enough: story page loads < 1s typical
* Safe: no exposure of private stories
* Abuse-resistant: rate limiting, reporting, moderation
* Maintainable: typed API, migrations, clean data model
* Scalable: support thousands of stories + reviews without redesign

---

## 7) Data Model (Postgres)

> Below is a concrete schema description. You can implement via Prisma/Drizzle/SQL migrations.

### 7.1 User

* id (uuid)
* email (unique)
* passwordHash (if not using OAuth-only)
* displayName
* username (unique, optional)
* avatarUrl (optional)
* bio (optional)
* roleLevel (optional: intern/junior/mid/senior/staff)
* createdAt, updatedAt
* isBanned (bool)
* reputationScore (int default 0)

### 7.2 Story

* id (uuid)
* authorId (fk User)
* title
* visibility (enum: PRIVATE, UNLISTED, PUBLIC)
* status (enum: DRAFT, PUBLISHED)
* storyType (enum or string: Conflict, Failure, Leadership, Ambiguity, etc.)
* industryTags (string[])
* competencyTags (string[])
* levelTag (string: mid/senior)
* rawText (rich text stored as markdown or HTML)
* structured fields:

  * situation (text)
  * task (text)
  * action (text)
  * result (text)
  * reflection (text)
* anonymizedText (optional)
* createdAt, updatedAt
* publishedAt (nullable)

### 7.3 StoryVersion (V1 recommended, optional MVP)

* id (uuid)
* storyId
* versionNumber (int)
* snapshot fields (same as Story main content)
* createdAt

### 7.4 Review

* id (uuid)
* storyId (fk)
* reviewerId (fk User)
* scores JSON:

  * clarity (1–5)
  * ownership (1–5)
  * impact (1–5)
  * decisionMaking (1–5)
  * communication (1–5)
  * reflection (1–5)
  * technicalDepth (1–5, optional)
* overallScore (computed or stored)
* comment (text)
* createdAt
* updatedAt
* isDeleted (bool)

Constraints:

* Unique (storyId, reviewerId) for MVP OR allow multiple reviews but rate-limit

### 7.5 CoverageItem (static config table OR code-defined)

Each checklist item:

* key (string unique) e.g. `has_metric`
* title
* description
* category (STAR / Impact / Communication / Leadership / Reflection)

### 7.6 StoryCoverage

* storyId
* coverageKey
* status (enum: MISSING, PRESENT, NOT_APPLICABLE)
* evidenceSnippet (optional)
* updatedAt

For MVP: author manually toggles checklist
For V1: auto-suggest + evidence

### 7.7 Report

* id
* reporterId
* targetType (enum: STORY, REVIEW, USER)
* targetId (uuid)
* reason (enum: SPAM, ABUSE, PERSONAL_INFO, PLAGIARISM, OTHER)
* details (text)
* createdAt
* status (enum: OPEN, RESOLVED, REJECTED)
* resolvedByAdminId (nullable)
* resolvedAt (nullable)

### 7.8 Reaction (optional MVP)

* id
* userId
* targetType (STORY/REVIEW)
* targetId
* type (UPVOTE, HELPFUL)
* createdAt
  Unique(userId, targetType, targetId)

---

## 8) API Design (REST or tRPC)

### Auth

* POST `/api/auth/signup`
* POST `/api/auth/login`
* POST `/api/auth/logout`
  (or NextAuth / Clerk / Auth0)

### Stories

* POST `/api/stories` create story
* GET `/api/stories/:id` read story (permission-aware)
* PATCH `/api/stories/:id` update story
* DELETE `/api/stories/:id` delete story
* POST `/api/stories/:id/publish`
* GET `/api/stories` list public stories (filters: tag, type, search, sort)
* GET `/api/me/stories` list user’s stories

Visibility rules:

* PRIVATE: only author
* UNLISTED: author + anyone with link (requires a `shareToken`)
* PUBLIC: everyone logged-in (or even logged-out if you want)

Implementation detail:

* For UNLISTED, add `shareToken` field on Story. Access allowed if token matches.

### Reviews

* POST `/api/stories/:id/reviews`
* GET `/api/stories/:id/reviews` (author sees all; others maybe see summary)
* PATCH `/api/reviews/:id` edit own review
* DELETE `/api/reviews/:id` soft delete

### Coverage

* GET `/api/stories/:id/coverage`
* PATCH `/api/stories/:id/coverage` bulk update

### Reports

* POST `/api/reports`
* GET `/api/admin/reports`
* PATCH `/api/admin/reports/:id/resolve`

---

## 9) Pages / UI Components

### Public

* Landing page
* Explore feed:

  * filters: story type, level, tags
  * sorts: newest, highest-rated, most-reviewed

### Authenticated

* Dashboard:

  * your stories list
  * “stories needing review” suggestions
* Story editor page:

  * structured STAR fields + rich text
  * tags selector
  * visibility selector
  * coverage checklist sidebar
* Story view page:

  * story content
  * coverage panel (author only)
  * review summary + charts (simple)
  * “Leave review” box
* Review form:

  * slider 1–5 per category
  * comment field
* Profile:

  * user info
  * reviewer reputation (helpful votes)

### Admin

* Reports list
* Content moderation controls

---

## 10) Story Scoring & Aggregation Logic

### Review aggregation

Compute per story:

* Average per rubric category
* Total average (weighted)
* Count of reviews
* “Confidence” score (more reviews => more stable)

Suggested weight (simple default):

* clarity 1.0
* ownership 1.2
* impact 1.2
* decisionMaking 1.0
* communication 1.0
* reflection 0.8
* technicalDepth 0.6 (optional)

### Reviewer reputation (optional)

Basic version:

* +1 reputation when your review is marked “Helpful”
* +X when you’ve reviewed N stories and your average helpfulness > threshold

---

## 11) Coverage Detection (MVP and V1)

### MVP: manual checklist + basic heuristics

Show checklist with toggles, default computed by simple rules:

* `has_metric`: detect numbers like `\d+%` or `$` or `ms` or `min`
* `uses_i_language`: ensure “I” appears in Action section
* `has_reflection`: reflection field non-empty
* `has_result`: result non-empty

Store computed suggestions but allow manual override.

### V1: AI-assisted coverage (optional)

Pipeline:

1. Take structured fields (Situation/Task/Action/Result/Reflection)
2. Run a classifier that outputs coverage status per key + evidence snippet
3. Save to StoryCoverage
4. UI shows:

   * what’s missing
   * “Add this” micro-suggestions

Safety:

* Never auto-publish private content
* Allow user to disable AI processing

---

## 12) Moderation & Safety

### Risks

* People paste private company info / names
* Toxic comments
* Spam reviews
* Plagiarism / low-effort content

### Safeguards (MVP)

* Rate limit:

  * max X reviews per day per user
  * max Y story publishes per day
* Report system
* Shadow banning for spam accounts
* “Do not include confidential information” warning on editor
* Optional “Anonymize” mode:

  * user can click “Anonymize” and app replaces detected names/emails/IDs

### Anti-abuse patterns

* require email verification
* captcha on signup / publish (optional)
* blocklist for obvious spam terms

---

## 13) System Architecture

### Recommended stack (fastest to ship)

* Frontend: Vue.js
* Backend: C# dotnet
* DB: MongoDb
* Cache: Redis (optional, later)
* Deployment: GCP

### Why this stack

* One repo, quick iteration
* Strong typing and schema migrations
* Easy to deploy and scale

---

## 14) Permissions & Access Control Rules (Important)

### Story access

* PRIVATE: only author
* UNLISTED: author OR token holder
* PUBLIC: any authenticated user (or public internet if you want)

### Review access

* Anyone who can view story can create review
* Only author can edit/delete own story
* Reviewer can edit/delete own review
* Author can hide abusive reviews only via reporting/moderation (to prevent bias)

### Data exposure

* Don’t leak private story titles in public endpoints
* `/api/stories` list endpoint must filter PUBLIC only

---

## 15) Performance Notes

### Heavy read endpoints

* Explore feed
* Story view page with review summary

Optimizations:

* Store computed `reviewCount`, `avgScores`, `overallScore` on Story table (denormalized)
* Update these on review create/update/delete via transaction
* Pagination on feed

---

## 16) Analytics (Optional but valuable)

Track:

* stories created per user
* publish rate
* review completion rate
* average review count per story
* “coverage completion” correlation with score improvement

This can be basic event logs or a 3rd party.

---

## 17) Testing Strategy

* Unit tests:

  * permissions checks
  * review aggregation math
  * coverage heuristic checks
* Integration tests:

  * story publish + feed visibility
  * unlisted share token access
* E2E tests:

  * create story → publish → receive review → view dashboard

---

## 18) Suggested MVP Implementation Plan (for the coding agent)

1. Setup C# backend + auth + database schema
2. Story CRUD + visibility handling
3. Explore feed (public stories)
4. Review system + aggregation
5. Coverage checklist + heuristics
6. Dashboard + story analytics
7. Reporting + admin tools
8. Rate limiting + polish

---

## 19) Acceptance Criteria (Definition of Done)

MVP is “done” when:

* A user can create private stories
* A user can publish a story and it appears in the feed
* Another user can review it with rubric scores + comment
* Author sees averages + coverage checklist
* Unlisted stories can be shared with link token
* Reports can be filed and admin can hide content

---

## 20) Future Enhancements (Backlog)

* Versioning + diffs
* AI rewrite suggestions with tone controls (concise / detailed / metric-heavy)
* “Interview loop pack” generator (Google / Amazon / Meta style)
* Private group mode (friends-only review circle)
* Export stories as PDF or flashcards
* Spaced repetition reminders (“review your 5 core stories weekly”)
