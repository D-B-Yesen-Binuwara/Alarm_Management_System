using INMS.Application.Services;
using INMS.Infrastructure.Persistence;
using INMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- DATABASE ----------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ---------------- SERVICES ----------------
builder.Services.AddScoped<INMS.Application.Services.CorrelationService>();
builder.Services.AddScoped<ImpactService>();

// ---------------- CONTROLLERS ----------------
builder.Services.AddControllers();

// ---------------- SWAGGER ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------------- DEVELOPMENT TOOLS ----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------- MIDDLEWARE ----------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ---------------- API ROUTES ----------------
app.MapControllers();

// ---------------- RUN APPLICATION ----------------
app.Run();