using Backend;
using Backend.Mappings;
using Backend.Policies;
using Backend.Repositories;
using Backend.Services;
using Backend.Validators;
using MongoDB.Driver;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

// Add Swagger services
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// mongodb section start
builder.Services.Configure<MongoDbSettings>( builder.Configuration.GetSection(nameof(MongoDbSettings)));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings?.ConnectionString);
});
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<MongoDbResiliencePolicy>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddSingleton<MongoDbStartupValidator>();
// mongodb section end

// Add this line with other services
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<MappingProfile>();
});

builder.Services.AddSingleton<PasswordHasher>();


var app = builder.Build();

// Validate MongoDB connection at startup
using (var scope = app.Services.CreateScope())
{
    var mongoValidator = scope.ServiceProvider.GetRequiredService<MongoDbStartupValidator>();
    await mongoValidator.ValidateConnectionAsync();
}

// Global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception?.Message);

        // once exception is caught, we return a generic error response to the client
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "An internal server error occurred",
            details = app.Environment.IsDevelopment() ? exception?.Message : null
        };

        await context.Response.WriteAsJsonAsync(response);

    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

// Enable CORS
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
