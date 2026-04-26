# ADNC Authentication and Authorization

[GitHub Repository](https://github.com/alphayu/adnc)

In the .NET Framework era, Forms Authentication was commonly used. However, for decoupled frontend-backend systems or multi-client architectures, Forms Authentication lacks compatibility, leading to low code reuse and high maintenance costs. ASP.NET Core has restructured authentication and authorization using a **Claims-based** model.

Commonly used authentication components include:

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Authentication.Cookies`
- `Microsoft.AspNetCore.Authentication.OpenIdConnect`
- `Microsoft.AspNetCore.Authentication.Auth`
- `Microsoft.AspNetCore.Authentication.Google`
- `Microsoft.AspNetCore.Authentication.Microsoft`
- `Microsoft.AspNetCore.Authentication.Facebook`
- `Microsoft.AspNetCore.Authentication.Twitter`

These components are provided by the .NET SDK and are unified based on claims, implementing abstractions from `Microsoft.AspNetCore.Authentication.Abstractions`. ASP.NET Core makes it easy to implement "Mixed Authentication," where different Actions in the same Controller can be configured to use Cookies or JwtBearer authentication.

To provide more flexible authentication strategies (e.g., supporting multiple schemes for a single Action), ADNC introduces **Hybrid** and **Basic** authentication modes. **Hybrid** is a project-specific routing scheme, while **Basic** follows the standard HTTP Basic Authentication. These implementations are located in the `Authentication` directory of the `Adnc.Shared.WebApi` project.

## Why Mixed Authentication?

Using only `JwtBearer` might be insufficient in certain scenarios, such as:

- **Synchronous Microservice Calls**: If Service A calls Service B's functionality, and only `JwtBearer` is used, the caller must possess permissions for both services, which complicates permission propagation and management.
- **Third-party Integration**: Exposing APIs to external parties may require simpler or alternative authentication methods.

For inter-service calls, **Basic Authentication** serves as a simpler, optional alternative. If your business scenario doesn't require this, you can stick to `JwtBearer`.

## Registering Authentication Services

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
        .AddJwtBearer(options => { })
        .AddBasic(options => options.Events.OnTokenValidated = (context) => { });
}
```

- **AddHybrid**: Registers services for Hybrid authentication.
- **AddBasic**: Registers services for Basic authentication.
- **AddJwtBearer**: Registers services for JwtBearer authentication.

The core of authentication lies in specific `AuthenticationHandlers`. In the example above, the default scheme is **Hybrid**. `HybridAuthenticationHandler` acts as a router that forwards requests to the appropriate authentication processor (e.g., JwtBearer or Basic).

## Registering Authorization Services
> Note: Authentication and Authorization are distinct concepts.

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

The core logic for authorization is in `AbstractPermissionHandler.cs` within the `Adnc.Shared.WebApi` project:

```csharp
public abstract class AbstractPermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated && context.Resource is HttpContext httpContext)
        {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            
            // If Basic authentication is used and the Action allows Basic, bypass further checks.
            if (authHeader != null && authHeader.StartsWith(BasicDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return;
            }

            // For JwtBearer, check for specific functional permissions.
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

## Setting Action Attributes

ADNC applies a global authorization requirement when registering Endpoint middleware, meaning all methods require authentication by default.

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireAuthorization();
});
```

- **Allow Anonymous Access**:
```csharp
[AllowAnonymous]
public async Task<ActionResult<UserTokenInfoDto>> LoginAsync([FromBody] UserLoginDto input)
```

- **Require Specific Permission (JwtBearer only)**:
```csharp
[Permission(PermissionConsts.Dept.Create)]
public async Task<ActionResult<long>> CreateAsync([FromBody] DeptCreationDto input)
```

- **Require Specific Permission (JwtBearer and Basic support)**:
```csharp
[Permission(PermissionConsts.Dept.GetList, PermissionAttribute.JwtWithBasicSchemes)]
public async Task<ActionResult<List<DeptTreeDto>>> GetListAsync()
```

## Specifying Authentication in Clients

- **JwtBearer** => `Authorization: Bearer {token}`
- **Basic** => `Authorization: Basic {credentials}`

Frontend applications typically use `JwtBearer`. Synchronous microservice calls (except for authentication services) often use `Basic` to simplify inter-service authorization.

- **Requesting JwtBearer**:
```csharp
[Headers("Authorization: Bearer")]
[Get("/usr/users/{userId}/permissions")]
Task<ApiResponse<List<string>>> GetCurrenUserPermissionsAsync(...);
```

- **Requesting Basic Auth**:
```csharp
[Get("/usr/depts")]
[Headers("Authorization: Basic")]
Task<ApiResponse<List<DeptRto>>> GeDeptsAsync();
```

---

*If this helps, please Star & Fork.*
