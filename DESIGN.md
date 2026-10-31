# Course Registration Design

## Scope and service boundary

`CourseRegistrationService` owns enrollment, dropping, and promotion decisions. The API controller translates HTTP requests into application DTOs; repositories provide EF Core queries; `RegistrationUnitOfWork` supplies the transaction and final persistence boundary. The feature covers one course offering per request. It does not include course/learner administration, identity, payments, terms, or a frontend.

## Data model

- `Learner`: identifier, name, and `HasRequiredPrerequisite` for the assessment's single prerequisite rule.
- `CourseOffering`: identifier, course code, capacity, active flag, and whether the prerequisite is required.
- `Enrollment`: learner/offering pair and enrollment timestamp.
- `WaitlistEntry`: learner/offering pair, queue timestamp, and active state. A promoted entry is retained as inactive for basic history.

EF Core configures foreign keys, a nonnegative-capacity check, a unique learner/offering enrollment index, a unique filtered index for active waitlist entries, and a queue-order index. FIFO is ordered by `CreatedAtUtc`, then database ID to make equal timestamps deterministic.

## Invariants and decisions

1. An offering cannot have more enrollments than its capacity through service operations.
2. A learner can have at most one enrollment for an offering and at most one active waitlist entry for it.
3. Only eligible learners can enroll or be promoted. For this prototype, eligibility means the prerequisite flag is true when the offering requires it.
4. An eligible learner enrolls immediately if a seat exists; otherwise they enter the queue. An ineligible learner is rejected and is not waitlisted.
5. On a drop, the seat release and promotion are committed together. The earliest eligible queue member is promoted. Ineligible entries are skipped for that promotion and remain active in their original queue position. If nobody is eligible, the seat remains open and the queue is unchanged.
6. A drop for a learner without an active enrollment returns a failure result and changes no state. A repeated drop therefore cannot release a second seat.

## Persistence and concurrency

The runnable prototype uses SQLite. Each enrollment, drop, or explicit promotion runs through `RegistrationUnitOfWork` in a serializable database transaction. Repository methods stage changes; the unit of work saves and commits once. A drop flushes its deletion inside the still-open transaction before querying the available-seat count, then stages any promotion before commit. SQLite integration tests include two independent contexts competing for one seat; the observed result is one enrollment and one waitlist entry.

Unique indexes provide a database-level backstop against duplicate enrollment and active waitlist rows. Capacity is a cross-row invariant, so it relies on serializing writes through the offering operation transaction rather than a simple database check constraint.

For deployment to SQL Server or a multi-instance service, use a provider-specific lock on the course-offering row (or another provider-supported serialization strategy), add transient transaction retry handling, and test competing requests against that production provider. SQLite's serializable transaction behavior is suitable for this small local prototype but is not a claim that provider behavior is identical across databases.

## API

- `POST /api/CourseRegistration/enroll`: accepts `courseOfferingId` and `learnerId`.
- `POST /api/CourseRegistration/drop`: accepts `courseOfferingId` and `learnerId`.

Both return `CourseRegistrationResult` with success flags and a human-readable message. Entity creation/seed endpoints are intentionally outside scope; learner and offering rows must already exist.

## Tradeoffs and limitations

- The prerequisite is represented by one boolean, not course-completion records or a prerequisite graph.
- Waitlist retention for ineligible learners is intentional and may require staff review; there is no notification or removal workflow.
- The API currently returns the application result with HTTP 200 for business failures as well as successes; a production API should map result kinds to explicit HTTP status codes.
- The database is created with EF Core `EnsureCreated`, not migrations. Schema evolution needs migrations before deployment.
- There are no course/learner provisioning endpoints or sample seed records.
- Automated tests use SQLite in-memory for service cases and a temporary SQLite file for the competing-request case. They do not prove concurrent behavior on a production SQL Server instance.