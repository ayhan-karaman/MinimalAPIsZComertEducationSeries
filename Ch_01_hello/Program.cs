var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string message = "Hello world. ";

app.MapGet("/hello", () => {
     return new Response(message);
})
.WithName("hello")
.WithOpenApi();

app.UseHttpsRedirection();

app.Run();

class Response
{
    public String? Message { get; set; }
    public DateTime Date => DateTime.Now;

    public Response(string? message)
    {
        Message = message;
        
    }
}
