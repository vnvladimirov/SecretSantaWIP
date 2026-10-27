using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Data;
using SecretSanta.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using SecretSanta.Security;
using Microsoft.AspNetCore.Authentication;

namespace SecretSanta;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddScoped<IDrawService, DrawService>();


        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Separate authentication cookie for Secret Santa participants.
        builder.Services
            .AddAuthentication()
            .AddCookie(ParticipantCookieDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "SecretSanta.Participant";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = false;
                options.LoginPath = "/ParticipantAccess/Enter";
                options.AccessDeniedPath = "/ParticipantAccess/Denied";

                options.Events.OnValidatePrincipal = async context =>
                {
                    var participantIdValue = context.Principal?
                        .FindFirst(ParticipantCookieDefaults.ParticipantIdClaim)?.Value;

                    var eventIdValue = context.Principal?
                        .FindFirst(ParticipantCookieDefaults.EventIdClaim)?.Value;

                    var invitationIdValue = context.Principal?
                        .FindFirst(ParticipantCookieDefaults.InvitationIdClaim)?.Value;

                    int participantId = 0;
                    int eventId = 0;
                    int invitationId = 0;

                    bool validClaims =
                        int.TryParse(participantIdValue, out participantId) &&
                        int.TryParse(eventIdValue, out eventId) &&
                        int.TryParse(invitationIdValue, out invitationId);

                    if (!validClaims)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(
                            ParticipantCookieDefaults.AuthenticationScheme);
                        return;
                    }

                    var db = context.HttpContext.RequestServices
                        .GetRequiredService<ApplicationDbContext>();

                    bool invitationIsValid = await db.ParticipantInvitations
                        .AnyAsync(i =>
                            i.Id == invitationId &&
                            i.ParticipantId == participantId &&
                            i.Participant.EventId == eventId &&
                            i.RevokedAtUtc == null &&
                            i.ExpiresAtUtc > DateTime.UtcNow);

                    if (!invitationIsValid)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(
                            ParticipantCookieDefaults.AuthenticationScheme);
                    }
                };

            });

        builder.Services.AddControllersWithViews();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            if (app.Environment.IsDevelopment())
            {
                DbSeeder.Seed(db);
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        app.Run();
    }
}
