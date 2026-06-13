using Microsoft.AspNetCore.Mvc;
using NetGuardGT.Api.Controllers;

namespace NetGuardGT.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Get_Redirects_To_Swagger()
    {
        var controller = new HomeController();

        var result = Assert.IsType<RedirectResult>(controller.Get());

        Assert.Equal("/swagger", result.Url);
    }
}
