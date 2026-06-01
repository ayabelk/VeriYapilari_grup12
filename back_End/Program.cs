var builder = WebApplication.CreateBuilder(args);

// ---- Servisler ----
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS: front_end ve a_i servisinin bu API'ye erişmesine izin ver
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()   // front_end, a_i, her yerden erişebilir
            .AllowAnyMethod()   // GET, POST, PUT, DELETE
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ---- Middleware sırası önemli! ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS, Authorization'dan ÖNCE gelmelidir
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "back_End API çalışıyor!");
app.Run();