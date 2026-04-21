public class UserResponseTests
{
    [Theory]
    [InlineData(17, false)]
    [InlineData(19, true)]
    public void From_MapsFieldsAndComputesIsOver18(int age, bool expectedIsOver18)
    {
        var user = new InMemoryUser(Guid.NewGuid(), "Aya", "Kovi", age);

        var response = UserResponse.From(user);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal("Aya", response.FirstName);
        Assert.Equal("Kovi", response.LastName);
        Assert.Equal(age, response.Age);
        Assert.Equal("Aya Kovi", response.FullName);
        Assert.Equal(expectedIsOver18, response.IsOver18);
    }
}

