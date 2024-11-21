using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var variable = () => "[Variable] Hello world.";

app.MapGet("/hello", () => "Hello world!");                // inline 
app.MapPost("/hello",  variable);                          // lambda variable
app.MapPut("/hello",  Hello);                              // local function
app.MapDelete("/hello",  new HelloHandler().Hello);        // instance member

String Hello()
{
    return "[Local Function] Hello world!";
}

app.Run();


class HelloHandler
{
     public string Hello() 
     {
         return "[Instance Member] Hello world!";
     }

}
