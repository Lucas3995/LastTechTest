namespace LastTechTest.API.Configuration;

public sealed class UserCreationOptions
{
    public const string SectionName = "UserCreation";
    public string DefaultPassword { get; set; } = "Trocar@123";
}
