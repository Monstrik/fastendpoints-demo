using FastEndpoints;

public sealed class ListPublicUsersEndpoint(IUserStore store) : EndpointWithoutRequest<List<PublicUserStatusResponse>>
{
    public override void Configure()
    {
        Get("/api/public/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = store.GetAll().Select(PublicUserStatusResponse.From).ToList();
        await Send.OkAsync(users, ct);
    }
}

