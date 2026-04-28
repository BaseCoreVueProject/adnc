# ADNC Authentication and Authorization

[GitHub repository](https://github.com/alphayu/adnc)

In the .NET Framework era, Forms authentication was commonly used. For systems with front-end/back-end separation and multiple clients, Forms authentication has poor compatibility and can easily lead to low code reuse and high maintenance cost.ASP.NET Core redesigned authentication and authorization around a claims-based authentication model. Common authentication components include:

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Authentication.Cookies`
- `Microsoft.AspNetCore.Authentication.OpenIdConnect`
- `Microsoft.AspNetCore.Authentication.Auth`
- `Microsoft.AspNetCore.Authentication.Google`
- `Microsoft.AspNetCore.Authentication.Microsoft`
- `Microsoft.AspNetCore.Authentication.Facebook`
- `Microsoft.AspNetCore.Authentication.Twitter`

These components are implemented by the .NET SDK, are all based on claims, and implement the abstractions in `Microsoft.AspNetCore.Authentication.Abstractions`. ASP.NET Core makes hybrid authentication straightforward: different actions in the same controller can use Cookies, JwtBearer, or other authentication schemes.To support more flexible authentication strategies, such as allowing the same action to support multiple authentication schemes, ADNC introduces Hybrid and Basic authentication. Hybrid is a project-specific routing solution, while Basic follows the HTTP standard. The implementation is located in the `Authentication` directory of the `Adnc.Shared.WebApi` project.

# Why Use Multiple Authentication Schemes?

In some scenarios, JwtBearer alone is not enough:

- Synchronous calls between microservices: if feature X in service A calls feature Y in service B, JwtBearer alone requires the caller to hold permissions for both service A and service B.
- The same issue may occur when exposing APIs to third parties.For synchronous calls between microservices, Basic authentication can be used as an optional and simpler authentication method. If your business scenario does not need this support, you can use JwtBearer only.For third-party integration scenarios, another authentication method can also be added.

# Register Authentication Services

```csharp
protected virtual void AddAuthentication<TAuthenticationHandler>() where TAuthenticationHandler : AbstractAuthenticationProcessor
{
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    var jwtSection = Configuration.GetRequiredSection(NodeConsts.JWT);
    var basicSection = Configuration.GetRequiredSection(NodeConsts.Basic);
    var jwtOptions = jwtSection.Get<JWTOptions>() ?? throw new InvalidDataException(nameof(jwtSection));
    Services
        .Configure<JWTOptions>(jwtSection)
        .Configure<BasicOptions>(basicSection)
        .AddScoped<AbstractAuthenticationProcessor, TAuthenticationHandler>()
        .AddAuthentication(HybridDefaults.AuthenticationScheme)
        .AddHybrid()
        .AddJwtBearer(options =>
        {
        })
        .AddBasic(options => options.Events.OnTokenValidated = (context) =>
        {
        });
}
```

- `AddHybrid`: registers Hybrid authentication services.
- `AddBasic`: registers Basic authentication services.
- `AddJwtBearer`: registers JwtBearer authentication services.The core authentication logic is implemented by each specific `AuthenticationHandler`. In the example above, the default authentication scheme is Hybrid. `HybridAuthenticationHandler` is responsible only for routing requests to the corresponding authentication processor and does not directly execute authentication logic.

- `HybridAuthenticationHandler`
- `JwtBearerAuthenticationHandler`
- `BasicAuthenticationHandler`

# Register Authorization Services

> Authentication and authorization are different concepts.

```csharp
public virtual void AddAuthorization<THandler>() where THandler : AbstractPermissionHandler
{
    _services.AddScoped<IAuthorizationHandler, THandler>();
    _services.AddAuthorization(options =>
    {
      options.AddPolicy(AuthorizePolicy.Default, policy =>
      {
        policy.Requirements.Add(new PermissionRequirement());
      });
    });
}
```

The core authorization code is in `AbstractPermissionHandler.cs` in the `Adnc.Shared.WebApi` project:

```csharp
public abstract class AbstractPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated && context.Resource is HttpContext httpContext)
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            // If Basic authentication is used and the action allows Basic, permission checks pass by default.
            if (authHeader != null && authHeader.StartsWith(BasicDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return;
            }

            // After JwtBearer authentication succeeds and the action allows JwtBearer,
            // the user's feature permissions still need to be checked.
            var userId = long.Parse(context.User.Claims.First(x => x.Type == JwtRegisteredClaimNames.NameId).Value);
            var validationVersion = context.User.Claims.FirstOrDefault(x => x.Type == "version")?.Value;
            var codes = httpContext.GetEndpoint().Metadata.GetMetadata<PermissionAttribute>().Codes;
            var result = await CheckUserPermissions(userId, codes, validationVersion);
            if (result)
            {
                context.Succeed(requirement);
                return;
            }
        }
        context.Fail();
    }

    protected abstract Task<bool> CheckUserPermissions(long userId, IEnumerable<string> codes, string validationVersion);
}
```

# Configure Action Properties

When ADNC registers Endpoint middleware, it sets global authentication interception, which means all methods require authentication by default.

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireAuthorization();
});
```

Allow anonymous access:

```csharp
[AllowAnonymous]
public async Task<ActionResult<UserTokenInfoDto>> LoginAsync([FromBody] UserLoginDto input)
```

Require the `PermissionConsts.Dept.Create` permission and the JwtBearer authentication scheme:

```csharp
[Permission(PermissionConsts.Dept.Create)]
public async Task<ActionResult<long>> CreateAsync([FromBody] DeptCreationDto input)
```

Require the `PermissionConsts.Dept.GetList` permission and support both JwtBearer and Basic authentication:

```csharp
[Permission(PermissionConsts.Dept.GetList, PermissionAttribute.JwtWithBasicSchemes)]
public async Task<ActionResult<List<DeptTreeDto>>> GetListAsync()
```

The following shows the implementation of `AdncAuthorizeAttribute`. The default authentication scheme is JwtBearer.

```csharp
public class AdncAuthorizeAttribute : AuthorizeAttribute
{
    public const string JwtWithBasicSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{Authentication.Basic.BasicDefaults.AuthenticationScheme}";

    public AdncAuthorizeAttribute(string code, string schemes = JwtBearerDefaults.AuthenticationScheme)
        : this([code], schemes)
    {
    }

    public AdncAuthorizeAttribute(string[] codes, string schemes = JwtBearerDefaults.AuthenticationScheme)
    {
        Codes = codes;
        Policy = AuthorizePolicy.Default;
        AuthenticationSchemes = schemes ?? throw new ArgumentNullException(nameof(schemes)); ;
    }

    public string[] Codes { get; set; }
}
```

# Specify Authentication from the Client

> JwtBearer => `Authorization: Bearer {token}`
> Basic => `Authorization: Basic {credentials}`
> Front-end interfaces usually use JwtBearer. Synchronous calls between microservices, except authentication services, usually use Basic authentication to simplify inter-service authentication.Require the server to use JwtBearer authentication:

```csharp
[Headers("Authorization: Bearer", "Cache: 2000")]
[Get("/usr/users/{userId}/permissions")]
Task<ApiResponse<List<string>>> GetCurrenUserPermissionsAsync(long userId, [Query(CollectionFormat.Multi)] IEnumerable<string> permissions, string validationVersion);
}
```

Require the server to use Basic authentication:

```csharp
[Get("/usr/depts")]
[Headers("Authorization: Basic", "Cache: 2000")]
Task<ApiResponse<List<DeptRto>>> GeDeptsAsync();
```

-- over --

If this helps, welcome to [star and fork](https://github.com/alphayu/adnc).
