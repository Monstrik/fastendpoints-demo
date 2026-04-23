using FastEndpoints;

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

