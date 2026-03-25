using CodeTestingService.Data;
using CodeTestingService.Models.Options;
using CodeTestingService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<GitLabResultsOptions>(options =>
{
    options.ServiceApiToken =
        builder.Configuration["SERVICE_API_TOKEN"]
        ?? builder.Configuration["GitlabResultsToken:Token"]
        ?? string.Empty;
    options.ServiceResultsUrl = builder.Configuration["SERVICE_RESULTS_URL"] ?? string.Empty;
});

builder.Services.AddSingleton<DemoCoreDataService>();
builder.Services.AddSingleton<DemoJwtUserService>();
builder.Services.AddSingleton<AccessService>();
builder.Services.AddScoped<RepositoryService>();
builder.Services.AddScoped<ResultsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapStaticAssets();
app.UseRouting();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(dbContext);
}

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
