using System.Security.Claims;
using FastEndpoints;

public sealed class CreateUserRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public sealed class UpdateUserRequest
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public string? Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public sealed class UserByIdRequest
{
    public Guid Id { get; set; }
}

public sealed class UpdateMyStatusRequest
{
    public required string Status { get; set; }
}

public sealed class UserResponse
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
    public required string Status { get; set; }

    public static UserResponse From(AppUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            FullName = user.FirstName + " " + user.LastName,
            Role = user.Role.ToString(),
            Status = user.Status
        };
    }
}

public sealed class CreateUserEndpoint(IUserStore store, IPasswordHasher passwordHasher) : Endpoint<CreateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Post("/api/users");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var created = store.Create(req.Login, passwordHasher.Hash(req.Password), req.FirstName, req.LastName, req.Age, req.Role);
        if (created is null)
        {
            AddError(r => r.Login, "Login already exists.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.CreatedAtAsync<GetUserByIdEndpoint>(new { id = created.Id }, UserResponse.From(created), cancellation: ct);
    }
}

public sealed class ListUsersEndpoint(IUserStore store) : EndpointWithoutRequest<List<UserResponse>>
{
    public override void Configure()
    {
        Get("/api/users");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = store.GetAll().Select(UserResponse.From).ToList();
        await Send.OkAsync(users, ct);
    }
}

public sealed class GetUserByIdEndpoint(IUserStore store) : Endpoint<UserByIdRequest, UserResponse>
{
    public override void Configure()
    {
        Get("/api/users/{id}");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(UserByIdRequest req, CancellationToken ct)
    {
        var user = store.GetById(req.Id);

        if (user is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(UserResponse.From(user), ct);
    }
}

public sealed class GetMyProfileEndpoint(IUserStore store) : EndpointWithoutRequest<UserResponse>
{
    public override void Configure()
    {
        Get("/api/me");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var id))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = store.GetById(id);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(UserResponse.From(user), ct);
    }
}

public sealed class UpdateMyStatusEndpoint(IUserStore store) : Endpoint<UpdateMyStatusRequest, UserResponse>
{
    public override void Configure()
    {
        Put("/api/me/status");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(UpdateMyStatusRequest req, CancellationToken ct)
    {
        if (!UserStatuses.Allowed.Contains(req.Status))
        {
            AddError(r => r.Status, "Status is not allowed.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idRaw, out var id))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var current = store.GetById(id);
        if (current is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = store.Update(
            current.Id,
            current.Login,
            null,
            current.FirstName,
            current.LastName,
            current.Age,
            current.Role,
            req.Status);

        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(UserResponse.From(updated), ct);
    }
}

public sealed class UpdateUserEndpoint(IUserStore store, IPasswordHasher passwordHasher) : Endpoint<UpdateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Put("/api/users/{id}");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var hashedPassword = string.IsNullOrWhiteSpace(req.Password) ? null : passwordHasher.Hash(req.Password);
        var updated = store.Update(req.Id, req.Login, hashedPassword, req.FirstName, req.LastName, req.Age, req.Role);

        if (updated is null)
        {
            AddError(r => r.Login, "User not found or login already exists.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(UserResponse.From(updated), ct);
    }
}

public sealed class DeleteUserEndpoint(IUserStore store) : Endpoint<UserByIdRequest>
{
    public override void Configure()
    {
        Delete("/api/users/{id}");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(UserByIdRequest req, CancellationToken ct)
    {
        if (!store.Delete(req.Id))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
