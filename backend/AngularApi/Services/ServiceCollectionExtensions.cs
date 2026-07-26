using AngularApi.Infrastructure;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace AngularApi.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthCookieOptions>(configuration.GetSection(AuthCookieOptions.SectionName));
            services.Configure<AppointmentSettings>(configuration.GetSection(AppointmentSettings.SectionName));
            services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
            services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>(); // should be addTrasient
            services.AddScoped<EmailTemplateService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IGoogleService, GoogleService>();
            services.AddScoped<IOwnershipValidator, OwnershipValidator>();
            services.AddScoped<IAuthCookieService, AuthCookieService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IMedicalCenterService, MedicalCenterService>();
            services.AddScoped<ISpecializationService, SpecializationService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<MedicalCenterDbContext>(option =>
            {
                option.UseSqlServer(ResolveSqlConnectionString(configuration));
            });
            services.AddScoped<IDatabaseMigrationRunner, EfCoreDatabaseMigrationRunner>();


            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
         .AddEntityFrameworkStores<MedicalCenterDbContext>()
         .AddDefaultTokenProviders();

        }

        public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var authCookieName = configuration["Jwt:AuthCookieName"] ?? "MedCenter.Auth";

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:ValidAudience"],
                    ValidateLifetime = true, //  Enforce expiration check
                    ClockSkew = TimeSpan.Zero,//  Prevents extra allowed time
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token)
                            && context.Request.Cookies.TryGetValue(authCookieName, out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["GoogleAuth:ClientId"];
                options.ClientSecret = configuration["GoogleAuth:ClientSecret"];
            });


            var corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();
            var allowedOrigins = corsSettings?.AllowedOrigins is { Length: > 0 }
                ? corsSettings.AllowedOrigins
                : CorsSettings.DefaultOrigins;

            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
        }


        public static void AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Medical Center API",
                    Description =
                        "REST API for the Medical Center healthcare platform. " +
                        "Manages patient and doctor profiles, appointment scheduling, payments, " +
                        "reviews, and administrative operations. " +
                        "Browser clients authenticate via HttpOnly JWT cookies; " +
                        "programmatic clients may use Bearer tokens in the Authorization header. " +
                        "Mutating requests require the X-XSRF-TOKEN antiforgery header when using cookies.",
                    Contact = new OpenApiContact
                    {
                        Name = "Medical Center Engineering",
                        Email = "engineering@medicalcenter.example"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "See repository LICENSE"
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "JWT Bearer authentication for API clients and tooling. " +
                        "Obtain a token via POST /api/Account/login or use a refresh token flow. " +
                        "Example: Authorization: Bearer {your JWT token}"
                });

                c.AddSecurityDefinition("AuthCookie", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Name = "MedCenter.Auth",
                    Description =
                        "HttpOnly JWT cookie used by the Angular SPA (default name: MedCenter.Auth). " +
                        "Set automatically on login; include cookies and X-XSRF-TOKEN for mutating requests."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "AuthCookie"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                c.TagActionsBy(api =>
                {
                    var controller = api.ActionDescriptor.RouteValues.TryGetValue("controller", out var name)
                        ? name
                        : "Other";
                    return [controller ?? "Other"];
                });
                c.DocInclusionPredicate((_, _) => true);
            });
        }
        public static async Task EnsureRolesCreatedAsync(this RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "admin", "user", "doctor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static string ResolveSqlConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("connection")
                ?? throw new InvalidOperationException("ConnectionStrings:connection is not configured.");

            if (connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
            {
                return connectionString;
            }

            var saPassword = configuration["ConnectionStrings:SaPassword"]
                ?? Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");

            if (string.IsNullOrWhiteSpace(saPassword))
            {
                throw new InvalidOperationException(
                    "SQL Server password is not configured. Mount /run/secrets/mssql_sa_password or set MSSQL_SA_PASSWORD.");
            }

            return $"{connectionString.TrimEnd(';')};Password={saPassword}";
        }
    }
}
