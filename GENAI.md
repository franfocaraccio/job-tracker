# GenAI

This document answers the Generative AI part of the exercise: the prompt I would use to generate a task management
REST API, a representative sample of the output, and how I validate, correct and harden what the AI produces.

**Tool:** [Claude Code](https://claude.com/claude-code).

---

## The prompt

The prompt is written to leave as little as possible to the model's imagination: context, constraints, exact data model,
business rules, security, error format, tests and an explicit "do not" list.

```text
You are a senior .NET engineer. Build a RESTful API for a simple task management system.

## Stack and constraints
- .NET 10, ASP.NET Core Web API with controllers (no Minimal APIs).
- Clean Architecture: Domain (entities + rules, no dependencies), Application (use cases + interfaces),
  Infrastructure (PostgreSQL), Api (controllers only translate HTTP <-> use cases).
- PostgreSQL accessed with plain ADO.NET through Npgsql and hand-written, parameterized SQL.
  Do NOT use an ORM.
- Keep it minimal: no extra features, layers, base classes, generic repositories or packages that are not needed.
- Name the C# types TaskItem and TaskItemStatus to avoid clashing with System.Threading.Tasks.Task/TaskStatus.

## Data model
A basic User already exists (users table with id uuid). Add a task:
- id: uuid, primary key
- user_id: uuid, FK to users, the owner of the task
- title: required, max 200 characters, trimmed
- description: optional, max 2000 characters
- status: one of Todo, InProgress, Done (stored as text, new tasks start as Todo)
- due_date: required, date only (no time, no time zone)

## Business rules
- A new task cannot have a due date in the past (today is allowed).
- When updating, a due date in the past is only rejected if it was changed (overdue tasks can still be edited).
- Domain entities must never be in an invalid state: private constructor, a Create factory that validates,
  and a FromDatabase factory to rebuild rows read from the database.

## API (all endpoints require a JWT; the user id comes from the "sub" claim, never from the request)
- GET    /api/tasks          -> 200, the user's tasks ordered by due date
- GET    /api/tasks/{id}     -> 200 | 404
- POST   /api/tasks          -> 201 with Location header | 400
- PUT    /api/tasks/{id}     -> 200 | 400 | 404
- DELETE /api/tasks/{id}     -> 204 | 404
- A task that belongs to another user must behave exactly like a missing one (404), never 403.
- Validation errors -> 400 with ProblemDetails. Missing/invalid token -> 401.
- Request/response DTOs are records in the Api project. Never return entities directly.

## Tests (write them first)
- xUnit unit tests for the domain rules and the use cases (NSubstitute for repositories).
- Integration tests for the repository against a real PostgreSQL (Testcontainers).
- Integration tests for the endpoints with WebApplicationFactory, including 401 and the ownership 404.

## Output
Work in small steps. For each step show the files you create or change and stop so I can review and commit.
If something in these instructions is ambiguous, ask before assuming.
```

**Prompt engineering choices**

- **Constraints over wishes.** "Do NOT use an ORM" and "keep it minimal" are explicit; models add abstractions by default.
- **Exact rules instead of "add validation".** Every rule is testable, so I can check the output against the prompt.
- **Security spelled out.** Ownership via the `sub` claim and "404, never 403" prevent the most common AI mistake in CRUD APIs:
  trusting a `userId` from the request.
- **Known pitfalls pre-empted.** Naming (`Task`/`TaskStatus` clash with .NET types) and dates (date-only, no time zone).
- **Small steps with review points.** I review and commit each step; a large one-shot output is much harder to verify.

## Output (representative sample)

Domain entity — rules live here and the entity can never be created in an invalid state:

```csharp
public enum TaskItemStatus
{
    Todo,
    InProgress,
    Done,
}

public sealed class TaskItem
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    private TaskItem(Guid id, Guid userId, string title, string? description, TaskItemStatus status, DateOnly dueDate)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Description = description;
        Status = status;
        DueDate = dueDate;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    public DateOnly DueDate { get; private set; }

    public static TaskItem Create(Guid userId, string title, string? description, DateOnly dueDate)
    {
        ValidateText(title, description);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dueDate < today)
        {
            throw new ValidationException("The due date cannot be in the past.");
        }

        return new TaskItem(Guid.NewGuid(), userId, title.Trim(), description?.Trim(), TaskItemStatus.Todo, dueDate);
    }

    public static TaskItem FromDatabase(Guid id, Guid userId, string title, string? description, TaskItemStatus status, DateOnly dueDate)
    {
        return new TaskItem(id, userId, title, description, status, dueDate);
    }

    public void Update(string title, string? description, TaskItemStatus status, DateOnly dueDate)
    {
        ValidateText(title, description);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dueDate != DueDate && dueDate < today)
        {
            throw new ValidationException("The due date cannot be moved to the past.");
        }

        Title = title.Trim();
        Description = description?.Trim();
        Status = status;
        DueDate = dueDate;
    }

    private static void ValidateText(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Title is required.");
        }

        if (title.Trim().Length > MaxTitleLength)
        {
            throw new ValidationException($"Title must be {MaxTitleLength} characters or fewer.");
        }

        if (description is not null && description.Trim().Length > MaxDescriptionLength)
        {
            throw new ValidationException($"Description must be {MaxDescriptionLength} characters or fewer.");
        }
    }
}
```

Repository — plain ADO.NET; ownership is enforced in the SQL itself:

```csharp
public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
{
    await using var command = _dataSource.CreateCommand(
        "SELECT id, user_id, title, description, status, due_date FROM tasks WHERE id = @id AND user_id = @userId");
    command.Parameters.AddWithValue("id", id);
    command.Parameters.AddWithValue("userId", userId);

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    if (!await reader.ReadAsync(cancellationToken))
    {
        return null;
    }

    return TaskItem.FromDatabase(
        reader.GetGuid(0),
        reader.GetGuid(1),
        reader.GetString(2),
        reader.IsDBNull(3) ? null : reader.GetString(3),
        Enum.Parse<TaskItemStatus>(reader.GetString(4)),
        reader.GetFieldValue<DateOnly>(5));
}
```

Controller — thin: reads the user from the token, calls the use case, translates the result to HTTP:

```csharp
[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _service;

    public TasksController(TaskService service)
    {
        _service = service;
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _service.UpdateAsync(
                id, GetUserId(), request.Title, request.Description, request.Status, request.DueDate, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(ToResponse(task));
        }
        catch (ValidationException exception)
        {
            return Problem(detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    }

    // GetAll, GetById, Create (201 + Location) and Delete (204) follow the same pattern.
}
```

Tests — the rules and the security behaviour are tested, not just the happy path:

```csharp
[Fact]
public void Create_WithPastDueDate_ThrowsValidationError()
{
    var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

    var exception = Assert.Throws<ValidationException>(
        () => TaskItem.Create(UserId, "Write the README", null, yesterday));

    Assert.Equal("The due date cannot be in the past.", exception.Message);
}

[Fact]
public void Update_OverdueTaskWithoutChangingTheDate_Succeeds()
{
    var overdue = TaskItem.FromDatabase(
        Guid.NewGuid(), UserId, "Old task", null, TaskItemStatus.Todo, new DateOnly(2020, 1, 1));

    overdue.Update("Old task", "Still pending", TaskItemStatus.InProgress, new DateOnly(2020, 1, 1));

    Assert.Equal(TaskItemStatus.InProgress, overdue.Status);
}

[Fact]
public async Task GetById_TaskOfAnotherUser_Returns404()
{
    var response = await _client.GetAsync($"/api/tasks/{taskOfAnotherUser.Id}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

## How I validate the AI's suggestions

I never accept generated code because it "looks right". For every step:
1. **Read every diff** in the IDE before committing, and ask myself whether I could explain each line in a code review.
2. **Build with warnings as errors** (`TreatWarningsAsErrors` + .NET analyzers) — the build must be clean.
3. **Run all the tests**, and check that new tests fail when the rule is broken (I break a rule on purpose to see a test go red).
4. **Run the app for real** — Swagger, curl and the browser against PostgreSQL, not only the in-memory tests.
5. **Security review**: SQL is parameterized, every query filters by the owner, no entity or password hash leaks into a response,
   the user id never comes from the request body.
6. **Research when in doubt** — official documentation instead of the model's opinion (e.g. Microsoft's guidance on testing data
   access, ASP.NET Core OpenAPI docs).

## What I correct or improve

These are the problems I look for in generated code. Each of them actually happened while building this repository:

| Problem in AI output | Correction |
|---|---|
| **Ownership not enforced** — queries by `id` only, or a `userId` taken from the request | Filter by `id AND user_id` in SQL; user id only from the JWT `sub` claim; another user's resource returns 404 |
| **Value-type defaults slip through** — a missing `DateOnly` in the JSON arrives as `0001-01-01` and passes "not in the future" checks | Explicit "date is required" rule plus a test |
| **Over-engineering** — custom exception hierarchies, base classes, injected clocks, helper methods used once | Use built-in types (`ValidationException`), inline the trivial code, delete what is not needed |
| **Code "for later" (YAGNI)** — methods with no caller yet | Remove them; add them in the step whose test needs them |
| **Dead code** — members only used by their own tests | Remove the member and its tests |
| **Scope creep** — dashboards, filters, extra fields, health endpoints nobody asked for | Keep strictly to the requirements |
| **Misleading names** — "unit tests" that start a database | Rename to `IntegrationTests` after checking Microsoft's guidance |

## Edge cases, authentication and validations

| Area | How it is handled |
|---|---|
| Authentication | JWT bearer on every task endpoint; missing, invalid or expired token → 401 |
| Authorization / ownership | User id from the `sub` claim; SQL filters by owner; another user's task → 404 (does not reveal it exists) |
| Required fields | Title required and trimmed; non-nullable request properties rejected by `[ApiController]` with 400 |
| Lengths | Title ≤ 200, description ≤ 2000, enforced in the domain and backed by the database column sizes |
| Status | Enum serialized as text; unknown values rejected with 400; stored as text with a `CHECK` constraint |
| Due date | Date only (no time-zone bugs); required; not in the past on create; overdue tasks can still be edited |
| Not found | `null` from the use case → 404 (no exceptions for control flow) |
| Errors | `ProblemDetails` with a clear message; no stack traces or internal details |
| SQL injection | Parameterized commands only |
| Concurrency of unique data | Database constraints are the last line of defence (`UNIQUE`, `CHECK`, FKs) |
