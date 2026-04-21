using FastEndpoints;

public sealed class CreateUserRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
}

public sealed class UpdateUserRequest
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
}

public sealed class UserByIdRequest
{
    public Guid Id { get; set; }
}

public sealed class UserResponse
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int Age { get; set; }
    public required string FullName { get; set; }
    public bool IsOver18 { get; set; }

    public static UserResponse From(InMemoryUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            FullName = user.FirstName + " " + user.LastName,
            IsOver18 = user.Age > 18
        };
    }
}

public sealed class CreateUserEndpoint(IUserStore store) : Endpoint<CreateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var created = store.Create(req.FirstName, req.LastName, req.Age);
        await Send.CreatedAtAsync<GetUserByIdEndpoint>(new { id = created.Id }, UserResponse.From(created), cancellation: ct);
    }
}

public sealed class ListUsersEndpoint(IUserStore store) : EndpointWithoutRequest<List<UserResponse>>
{
    public override void Configure()
    {
        Get("/api/users");
        AllowAnonymous();
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
        AllowAnonymous();
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

public sealed class UpdateUserEndpoint(IUserStore store) : Endpoint<UpdateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Put("/api/users/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var updated = store.Update(req.Id, req.FirstName, req.LastName, req.Age);

        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
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
        AllowAnonymous();
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

