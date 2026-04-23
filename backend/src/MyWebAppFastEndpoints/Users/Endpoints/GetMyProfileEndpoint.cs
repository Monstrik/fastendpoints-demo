using System.Security.Claims;
using FastEndpoints;

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

