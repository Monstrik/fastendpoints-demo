using FastEndpoints;

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
        var updated = store.Update(req.Id, req.Login, hashedPassword, req.FirstName, req.LastName, req.Role);

        if (updated is null)
        {
            AddError(r => r.Login, "User not found or login already exists.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(UserResponse.From(updated), ct);
    }
}

