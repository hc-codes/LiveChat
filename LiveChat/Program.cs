using LiveChat.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(); // Add SignalR service
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5501", "http://192.168.1.10:5501") // your frontend address (important!!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // <- important
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat"); // Map the ChatHub endpoint

app.Run();