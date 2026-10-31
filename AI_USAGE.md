# AI Assistance, Prompt Log, and Verification

**Project:** Course Registration Service  
**Technology:** ASP.NET Core, EF Core, SQLite, xUnit  
**Record scope:** User-provided prompts, AI-assisted decisions, implementation outcomes, and verification evidence.

This document distinguishes historical prompts from editorial material. The archived text below is quoted from the conversation. The reusable implementation brief is a curated synthesis and was not sent as a verbatim prompt. This record supports review; it does not replace human approval of business rules or production readiness.

## 1. Verbatim Prompt Archive

### P-01 | Initial requirements and project setup

P-01 groups the initial details from several consecutive user messages. The messages remain separate blocks below so their original wording and sequence are clear.

```text
help me to create this suggest tech stack as per my analysis this is the context "
I’ve reviewed the Course Registration: AI-Assisted Engineering Assessment document. I understand the requirements, scope, acceptance criteria, and expected deliverables. Before we start implementation, here is my understanding of what needs to be done.

## 1. What we need to build
We need to implement a small, reliable course-registration feature for one course offering in the provided starter project.

The feature must support three core operations:

1. Enroll a learner

If seats are available and the learner satisfies the prerequisite rule, enroll them. If the course is full, add an eligible learner to the waitlist.

2. Maintain the waitlist

Allow eligible learners to join the queue once, without duplicate enrollment or waitlist records.

3. Drop and promote

When an enrolled learner drops the course, free the seat and promote the earliest eligible learner according to the documented FIFO waitlist policy.

## 2. Important business rules
These rules must be implemented and verified with automated tests.

Scenario

Expected behavior

Seat available

Enroll an eligible learner.

Course full

Add the eligible learner to the waitlist.

Duplicate enrollment

Do not create duplicate records.

Learner drops

Release the seat and promote the next eligible learner.

Prerequisite failure

Do not enroll or promote the ineligible learner.

Ineligible learner at queue front

Apply our documented skip-or-retain policy consistently.

Invalid or repeated drop

Return a clear result without corrupting state.

Concurrent requests

Preserve the starter project's transaction or locking pattern, where applicable.

One important decision is how to handle an ineligible learner at the front of the queue. The assessment requires us to state this assumption and apply it consistently. We should not leave this behavior undefined.

## 3. What we must deliver
The assessment requires more than just working code.

Runnable implementation

The enrollment, drop, and FIFO promotion logic, integrated with the supplied starter project's existing design.

Automated tests

Tests covering available and full capacity, duplicate requests, FIFO promotion, prerequisite failures, and invalid or repeated drops.

Design specification

Explain the service boundary, relevant data/API design, capacity invariants, waitlist rules, persistence or concurrency approach, assumptions, and tradeoffs.

AI usage and verification record

Record the prompts used, relevant AI suggestions, whether each was accepted, edited, or rejected, which files or decisions were affected, and how the changes were independently verified.

Setup and execution instructions

Provide the commands needed to run the application and automated tests.

Limitations and follow-up risks

Briefly document any issues or risks identified within the feature's scope.

## 4. What we should not build
The document explicitly limits the scope. We should not add:

- A frontend or complete course catalog.
- Payments or identity integrations.
- Multi-term registration.
- A new database unless the starter project requires one.
- A configurable policy engine.
- Separate greenfield, brownfield, and ambiguous implementations.

If no starter project is supplied, the document permits a minimal service or command-line prototype with an in-memory store.

## 5. My proposed approach
I recommend following this sequence so we can produce a solution that is both correct and easy to review.

1. Inspect the starter project. Understand its existing interfaces, models, persistence, test framework, and coding conventions before making changes.
2. Document the assumptions. Define prerequisite eligibility, duplicate handling, FIFO promotion, and what happens to an ineligible learner in the queue.
3. Design the service. Establish the enrollment and capacity invariants and determine how state changes will remain consistent.
4. Implement the feature. Make the smallest set of changes necessary to satisfy the requirements.
5. Write and execute tests. Verify normal cases, edge cases, and concurrency behavior if applicable.
6. Document AI assistance and verification. Keep an honest record of AI contributions and the evidence supporting our decisions.
7. Package the deliverables. Include runnable code, tests, design specification, AI usage record, setup instructions, and known limitations.

The suggested timebox is 7 to 8 hours, with priority given to correctness, focused testing, and clear evidence of independent review."
```

**Stack**

my tech is "- .NET-based → use `ASP.NET Core + EF Core + xUnit`"


**Design planning request**

```text
create plan to draft:

1. the exact .NET project structure,
2. the domain model,
3. the service/test design for this course registration feature.
```

**Project location**

```text
add project here "C:\Users\aniru\Downloads\hareesh"
```

### P-02 | Implementation and deliverables request

```text
1. Implementing the actual seat/waitlist business rules
2. Writing the required xUnit tests for the assessment scenarios
3. Draft the design document and AI usage record
```

## 2. Curated Implementation Brief

The following brief consolidates the requirements into a reusable, review-oriented prompt. It is an editorial synthesis of P-01 and P-02, **not an exact prompt sent during the original conversation**.

```text
Role
Act as a senior .NET engineer implementing a narrowly scoped course-registration feature in the existing starter solution. Preserve its conventions and avoid unrelated changes.

Context
The feature handles one course offering. Use the existing .NET stack: ASP.NET Core, EF Core, and xUnit. Inspect the supplied starter's models, persistence, transaction pattern, and tests before changing code. If no starter project or database is supplied, follow the assessment's permitted minimal-service/in-memory option.

Required behavior
1. Enroll an eligible learner when capacity is available.
2. When full, add an eligible learner to the FIFO waitlist exactly once.
3. Reject prerequisite-ineligible learners without creating enrollment or waitlist records.
4. On drop, release the seat and promote the earliest eligible learner atomically.
5. Prevent duplicate enrollment and active waitlist records.
6. Return a clear result for invalid or repeated drops without corrupting state.
7. Define and consistently implement whether an ineligible learner at the queue front is skipped or retained.
8. Preserve capacity under concurrent requests using the starter project's persistence and locking approach where available.

Scope constraints
Do not add a frontend, catalog, payments, identity integration, multi-term support, a configurable policy engine, unrelated implementations, or a new database unless the starter requires it. If a relational test provider appears useful, explain the tradeoff and flag any scope deviation for approval instead of silently expanding scope.

Deliverables
Provide runnable implementation; focused automated tests for normal, edge, and applicable concurrency cases; a design specification covering service boundary, data/API model, invariants, waitlist policy, persistence, concurrency, assumptions, and tradeoffs; setup and execution commands; an honest AI-use and verification record; and concise limitations/follow-up risks.

Verification
Run the relevant test suite and build. Report the exact commands, outcomes, warnings, and any behavior not verified. Do not claim production concurrency guarantees based only on a different database provider.
```

## 3. AI Decision and Change Log

### P-01 | Initial requests

- **AI contribution:** Recommended a layered ASP.NET Core + EF Core + xUnit design.
- **Disposition:** Accepted; adapted to the .NET 10 scaffold created in the workspace because no starter project was present. SQLite was then introduced to test relational transactions and constraints. This differs from the assessment's permitted in-memory fallback and should be confirmed as acceptable; switching to in-memory storage or a minimal CLI would align more strictly if new-database scope is not allowed.
- **Affected areas:** Solution structure and project dependencies.
- **Evidence:** Full solution build and test run recorded below.

**Design, skeleton, and project setup**

- **AI contribution:** Proposed API, Application, Domain, Infrastructure, and Tests projects, with registration service and repository boundaries.
- **Disposition:** Used as the scaffold. Placeholder eligibility and independent repository saves were replaced with explicit prerequisite handling and a transactional unit of work.
- **Affected areas:** API, Application, Domain, Infrastructure, Tests.
- **Evidence:** SQLite-backed service tests and full solution test run.

### P-02 | Implementation and assessment deliverables

- **AI contribution:** Implemented duplicate checks, capacity-based enrollment/waitlisting, prerequisite checks, atomic drop/promotion, FIFO ordering, and the explicit skip-but-retain policy for ineligible queued learners.
- **Disposition:** Implemented. The prerequisite is represented as a boolean for this prototype; the ineligible learner is skipped for a promotion attempt but remains queued.
- **Affected areas:** `CourseRegistrationService`, `Learner`, `CourseOffering`, repository interfaces and implementations, `RegistrationUnitOfWork`, EF model, tests, `DESIGN.md`, and `README.md`.
- **Evidence:** 13 SQLite-backed tests passed, including a separate-context competing-enrollment test.

### Verification and independent review boundary

The following commands were run during implementation:

```bash
dotnet test "C:\Users\aniru\Downloads\hareesh\CourseRegistrationService\CourseRegistration.Tests\CourseRegistration.Tests.csproj"
```

Final command:

```bash
dotnet test "C:\Users\aniru\Downloads\hareesh\CourseRegistrationService\CourseRegistration.sln"
```

**Final result:** 13 tests passed, 0 failed; all solution projects built successfully. Coverage includes available seats, full offerings, duplicate enrollment and waitlisting, prerequisite rejection, FIFO promotion, skip-and-retain behavior, invalid/repeated drops, empty eligible queues, prerequisite-free offerings, and concurrent requests for one seat.

The checks were executed by the AI coding agent against the project test suite. They are independent behavioral checks of the implementation, but they are **not a human code review** and do not verify deployment-provider behavior.

NuGet reported six warnings in the final run, including high-severity advisories for `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 and `Microsoft.OpenApi` 2.0.0. These dependency risks remain unresolved and should be addressed before deployment. The final run verified the code after the implementation and documentation changes.

## 4. Human Review and Follow-up

- Confirm the assessment's prerequisite definition and whether the skip-but-retain policy is acceptable.
- Confirm whether SQLite is acceptable despite the assessment's in-memory fallback for an absent starter; otherwise replace persistence with the permitted minimal in-memory implementation.
- Review transaction/locking behavior against the intended production database, especially under concurrent enrollment attempts.
- Decide HTTP status mapping, database migrations, and dependency advisory remediation before deployment.