public sealed class CreateUserRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public sealed class UpdateUserRequest
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public string? Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public sealed class UserByIdRequest
{
    public Guid Id { get; set; }
}

public sealed class UpdateMyStatusRequest
{
    public required string Status { get; set; }
}

