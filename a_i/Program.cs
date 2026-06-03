var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())        
    app.MapOpenApi();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "a_i API çalışıyor!");
app.Run();