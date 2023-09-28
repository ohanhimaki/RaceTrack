using RaceTrack.Core;
using RaceTrack.Core.Messaging;
using RaceTrack.Core.Services;
using RaceTrack.Video.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RaceManagerDbService>();
builder.Services.AddSingleton<EventAggregator>();
builder.Services.AddSingleton<IVideoCaptureService, VideoCaptureService>();
builder.Services.AddSingleton<RaceManager>(); // todo vaihda singleton

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();