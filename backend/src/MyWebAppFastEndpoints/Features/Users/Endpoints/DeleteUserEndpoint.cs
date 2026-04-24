using FastEndpoints;

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

