public sealed class UserResponse
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string FullName { get; set; }
    public required string Role { get; set; }
    public required string Status { get; set; }

    public static UserResponse From(AppUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FirstName + " " + user.LastName,
            Role = user.Role.ToString(),
            Status = user.Status
        };
    }
}

public sealed class PublicUserStatusResponse
{
    public required string Login { get; set; }
    public required string FullName { get; set; }
    public required string Status { get; set; }

    public static PublicUserStatusResponse From(AppUser user)
    {
        return new PublicUserStatusResponse
        {
            Login = user.Login,
            FullName = user.FirstName + " " + user.LastName,
            Status = user.Status
        };
    }
}

