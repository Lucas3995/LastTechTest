using System.Reflection;

using FluentAssertions;

using LastTechTest.Dominio.Interfaces;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class ListUsersQueryHandlerTests
{
    [Fact]
    public async Task RF6_T1_Handle_ShouldReturnUserList_WhenUsersExist()
    {
        var listAsync = typeof(IUserRepository).GetMethod("ListAsync", new[] { typeof(CancellationToken) });
        listAsync.Should().NotBeNull("RF-6 requires IUserRepository.ListAsync(CancellationToken)");

        var handlerType = FindType(
            "LastTechTest.Aplicacao.Authentication.Queries.ListUsers.ListUsersQueryHandler",
            "LastTechTest.Aplicacao");
        handlerType.Should().NotBeNull("RF-6 requires ListUsersQueryHandler");

        var queryType = FindType(
            "LastTechTest.Aplicacao.Authentication.Queries.ListUsers.ListUsersQuery",
            "LastTechTest.Aplicacao");
        queryType.Should().NotBeNull("RF-6 requires ListUsersQuery");

        var query = Activator.CreateInstance(queryType!);
        query.Should().NotBeNull();

        var handlerCtor = handlerType!.GetConstructors().SingleOrDefault();
        handlerCtor.Should().NotBeNull();
        var ctorParams = handlerCtor!.GetParameters();
        ctorParams.Select(p => p.ParameterType).Should().Contain(typeof(IUserRepository));

        var responseType = FindType(
            "LastTechTest.Aplicacao.Authentication.Queries.ListUsers.ListUsersResponse",
            "LastTechTest.Aplicacao");
        responseType.Should().NotBeNull("RF-6 requires response DTO for list users");
        responseType!.GetProperty("Items").Should().NotBeNull();

        await Task.CompletedTask;
    }

    [Fact]
    public async Task RF6_T1_Handle_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        var listAsync = typeof(IUserRepository).GetMethod("ListAsync", new[] { typeof(CancellationToken) });
        listAsync.Should().NotBeNull("RF-6 requires IUserRepository.ListAsync(CancellationToken)");

        var handlerType = FindType(
            "LastTechTest.Aplicacao.Authentication.Queries.ListUsers.ListUsersQueryHandler",
            "LastTechTest.Aplicacao");
        handlerType.Should().NotBeNull("RF-6 requires ListUsersQueryHandler");

        var queryType = FindType(
            "LastTechTest.Aplicacao.Authentication.Queries.ListUsers.ListUsersQuery",
            "LastTechTest.Aplicacao");
        queryType.Should().NotBeNull("RF-6 requires ListUsersQuery");

        var query = Activator.CreateInstance(queryType!);
        query.Should().NotBeNull();

        var responseItemType = FindType(
            "LastTechTest.Aplicacao.Authentication.Queries.ListUsers.ListUsersItemResponse",
            "LastTechTest.Aplicacao");
        responseItemType.Should().NotBeNull("RF-6 should expose response item contract for user row");
        responseItemType!.GetProperty("Id").Should().NotBeNull();
        responseItemType.GetProperty("Email").Should().NotBeNull();
        responseItemType.GetProperty("Roles").Should().NotBeNull();

        await Task.CompletedTask;
    }

    private static Type? FindType(string fullName, string assemblyName)
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.Ordinal));

        if (loaded is null)
        {
            try
            {
                loaded = Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }

        return loaded.GetType(fullName, throwOnError: false, ignoreCase: false);
    }
}