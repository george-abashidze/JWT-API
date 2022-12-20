using JWT_API.Data;
using JWT_API.Data.Entities;
using JWT_API.Enums;
using JWT_API.Helpers;
using JWT_API.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//poweruser config
var powerUserConfig = builder.Configuration.GetSection("PowerUser").Get<PowerUserConfig>();

//jwt config
var jwtTokenConfig = builder.Configuration.GetSection("jwtTokenConfig").Get<JwtTokenConfig>();


if (jwtTokenConfig == null || powerUserConfig == null)
    return;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (String.IsNullOrEmpty(connectionString))
    return;

builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton<PowerUserConfig>(powerUserConfig);
builder.Services.AddSingleton<JwtTokenConfig>(jwtTokenConfig);

    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    }).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = true;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtTokenConfig.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
            ValidAudience = jwtTokenConfig.Audience,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddSingleton<IJwtAuthManager, JWTAuthManager>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// ---------------------- roles setup ----------------

using var scope = app.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//creates all roles from Role enum, if not exists
foreach (var role in (Role[])Enum.GetValues(typeof(Role)))
{
    string? enumName = Enum.GetName(typeof(Role), role);

    var roleExist = await context.Roles.AnyAsync(r => r.Name.Equals(enumName));
    if (!roleExist)
    {
        await context.Roles.AddAsync(new RoleEntity
        {
            Name = enumName,
        });
    }
}

await context.SaveChangesAsync();

// --------------- END of roles setup

// --------------- power user setup ------------

var poweruser = new UserEntity
{
    Email = powerUserConfig.Email.Trim().ToLower(),
    FirstName = powerUserConfig.FirstName,
    PhoneNumber = powerUserConfig.PhoneNumber,
    PasswordHash = PasswordHelper.GetStringHash(powerUserConfig.Password, jwtTokenConfig.Secret),
    LastName = powerUserConfig.LastName
};

var _user = await context.Users.Include(u => u.Roles).AnyAsync(u => u.Roles.Any(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.PowerUser))));
//create power user with PowerUser role, if not exists
if (!_user)
{
    var powerUserRole = await context.Roles.FirstAsync(r => r.Name.Equals(Enum.GetName(typeof(Role), Role.PowerUser)));
    poweruser.Roles = new[] { powerUserRole };
    await context.Users.AddAsync(poweruser);
}

await context.SaveChangesAsync();

// ---------------- END of power user setup ------------------

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();
app.UseMiddleware<JWTMiddleware>();
app.MapControllers();

app.Run();
