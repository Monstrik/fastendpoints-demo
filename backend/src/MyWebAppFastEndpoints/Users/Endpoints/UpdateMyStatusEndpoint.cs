using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Updates the authenticated user's status to a valid predefined value.
/// </summary>
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

        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var current = store.GetById(userId.Value);
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
