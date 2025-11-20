using System.Text;
using BussinessObject;
using BookingVilla.Services;
using BookingVilla.Services.OData;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;

namespace BookingVilla
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Thiết lập DbContext với SQL Server
            var connectionString = builder.Configuration.GetConnectionString("BookingVillaPRN") 
                ?? throw new InvalidOperationException("Connection string 'BookingVillaPRN' not found.");
            
            builder.Services.AddDbContext<BookingVillaPrnContext>(options =>
                options.UseSqlServer(connectionString));

            // Đăng ký Repository và DAO
            builder.Services.AddScoped<IVillaRepositories, VillaRepositories>();
            builder.Services.AddScoped<IServiceRepositories, ServiceRepositories>();
            builder.Services.AddScoped<IAccountRepositories, AccountRepositories>();
            builder.Services.AddScoped<IBookingRepositories, BookingRepositories>();
            builder.Services.AddScoped<IBookingHistoryRepositories, BookingHistoryRepositories>();
            builder.Services.AddScoped<ICancelBookingRepositories, CancelBookingRepositories>();
            builder.Services.AddScoped<IEmployeeRepositories, EmployeeRepositories>();
            builder.Services.AddScoped<ITransactionRepositories, TransactionRepositories>();
            builder.Services.AddScoped<ICustomerRepositories, CustomerRepository>();

            builder.Services.AddScoped<EmployeeDAO>();

            // Đăng ký JWT Service
            builder.Services.AddScoped<IJwtService, JwtService>();

            // Cấu hình JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "BookingVilla",
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"] ?? "BookingVillaUsers",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Cấu hình Authorization với Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("ad"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("us"));
                options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("em"));
                options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("us", "ad"));
            });

            // Cấu hình CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                // For production, use specific origins
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Cấu hình API Controllers (cho Web API) + OData
            builder.Services.AddControllers()
                .AddOData(options =>
                    options.AddRouteComponents("odata", GetEdmModel())
                           .Select()
                           .Filter()
                           .OrderBy()
                           .Expand()
                           .Count()
                           .SetMaxTop(null)
                );
            builder.Services.AddEndpointsApiExplorer();

            // Cấu hình Razor Pages (cho Web UI)
            builder.Services.AddRazorPages();

            // Cấu hình Swagger/OpenAPI (cho Web API)
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Booking Villa API",
                    Version = "v1",
                    Description = "API for Booking Villa application with JWT Authentication"
                });

                // Cấu hình JWT trong Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
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
                    }
                });
            });

            // Cấu hình Session (cho Razor Pages)
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Cấu hình SignalR
            builder.Services.AddSignalR();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient("ODataClient");
            builder.Services.AddScoped<IODataClient, ODataClient>();

            var app = builder.Build();

            // Cấu hình Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking Villa API v1");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Phục vụ file tĩnh (cho Razor Pages)
            app.UseStaticFiles();

            // CORS phải được đặt trước UseAuthentication và UseAuthorization
            app.UseCors("AllowAll"); // Hoặc "AllowSpecificOrigins" cho production

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Session middleware (cho Razor Pages)
            app.UseSession();

            // Map Swagger endpoints trước (nếu cần explicit routing)
            // Map API Controllers (Web API endpoints) - Phải map trước Razor Pages
            app.MapControllers();

            // Map Razor Pages (Web UI) - Map sau để không conflict với API routes
            app.MapRazorPages();

            // Map SignalR Hub
            app.MapHub<NewsHub>("/newsHub");

            app.Run();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            
            // Cấu hình Entity Sets với key properties rõ ràng
            var villasEntitySet = builder.EntitySet<Villa>("Villas");
            villasEntitySet.EntityType.HasKey(v => v.IdVilla);
            
            var bookingOnlinesEntitySet = builder.EntitySet<BookingOnline>("BookingOnlines");
            bookingOnlinesEntitySet.EntityType.HasKey(b => b.IdBookingOnline);
            
            var customersEntitySet = builder.EntitySet<Customer>("Customers");
            customersEntitySet.EntityType.HasKey(c => c.IdCustomer);
            
            var servicesEntitySet = builder.EntitySet<Service>("Services");
            servicesEntitySet.EntityType.HasKey(s => s.IdService);
            
            var employeesEntitySet = builder.EntitySet<Employee>("Employees");
            employeesEntitySet.EntityType.HasKey(e => e.IdEmployee);
            
            var discountsEntitySet = builder.EntitySet<Discount>("Discounts");
            discountsEntitySet.EntityType.HasKey(d => d.IdDiscount);
            
            var transactionsEntitySet = builder.EntitySet<Transaction>("Transactions");
            transactionsEntitySet.EntityType.HasKey(t => t.IdTransactions);
            
            return builder.GetEdmModel();
        }
    }
}
