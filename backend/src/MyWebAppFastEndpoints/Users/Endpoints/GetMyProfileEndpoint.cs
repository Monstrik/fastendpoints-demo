using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Retrieves the authenticated user's profile information.
/// </summary>
public sealed class GetMyProfileEndpoint(IUserStore store) : EndpointWithoutRequest<UserResponse>
{
    public override void Configure()
    {
        Get("/api/me");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = store.GetById(userId.Value);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(UserResponse.From(user), ct);
    }
}
