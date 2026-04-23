public class UserResponseTests
{
    [Fact]
    public void From_MapsFields()
    {
        var user = new AppUser(Guid.NewGuid(), "aya", "hash", "Aya", "Kovi", UserRole.User);

        var response = UserResponse.From(user);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal("aya", response.Login);
        Assert.Equal("Aya", response.FirstName);
        Assert.Equal("Kovi", response.LastName);
        Assert.Equal("Aya Kovi", response.FullName);
        Assert.Equal("User", response.Role);
        Assert.Equal(UserStatuses.Default, response.Status);
    }
}
