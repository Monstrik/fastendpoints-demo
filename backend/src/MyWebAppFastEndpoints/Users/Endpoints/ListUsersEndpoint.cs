using FastEndpoints;

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

