using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TestAPI.Attributes;



/// <summary>
/// Custom Attribute that checks if the user is valid and access key is also valid
/// ADD THIS TO CONTROLLERS [CheckValid(<role>)]
/// </summary>
public class CheckValid : AuthorizeAttribute, IAuthorizationFilter
{
    // Role to check
    private readonly string _role;

    // [CheckValid(<role>)]
    public CheckValid(string role)
    {
        _role = role;
    }

    // On Authorization, call this
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Get the user from the HttpContext
        var user = context.HttpContext.User;

        // if user is authenticated
        bool isUserAuthenticated = user.Identity.IsAuthenticated;

        // if user does not have the role specified on the _role variable
        bool isRole = user.IsInRole(_role);

        // if access token is not added to header
        bool containsAuthToken = context.HttpContext.Request.Headers.TryGetValue("Authorization", out var accessToken);

        // set up claim value for expiration date
        var token = accessToken.ToString().Split(" ").Last();
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var exp = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "exp")!.Value;
        var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime;

        bool isExpired = DateTime.UtcNow.CompareTo(expDate) >= 0;

        if (!isUserAuthenticated || !isRole || !containsAuthToken || isExpired)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

    }
}
