using FastEndpoints;

public sealed class CreateUserEndpoint(IUserStore store, IPasswordHasher passwordHasher) : Endpoint<CreateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Post("/api/users");
        Roles(UserRole.Admin.ToString());
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var created = store.Create(req.Login, passwordHasher.Hash(req.Password), req.FirstName, req.LastName, req.Role);
        if (created is null)
        {
            AddError(r => r.Login, "Login already exists.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.CreatedAtAsync<GetUserByIdEndpoint>(new { id = created.Id }, UserResponse.From(created), cancellation: ct);
    }
}

