using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniValidation;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
       b => b.MigrationsAssembly("DemoMinimalAPI")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthConfiguration();
app.UseHttpsRedirection();


app.MapGet("/fornecedor", async (MinimalContextDb contextDb) =>

    await contextDb.fornecedors.ToListAsync()
) .WithName("GetFornecedor")
  .WithTags("Fornecedor");

app.MapGet("/fornecedor/{id}", async (Guid id, MinimalContextDb contextDb) =>

    await contextDb.fornecedors.FindAsync(id)
    is Fornecedor fornecedor ? Results.Ok(fornecedor) : Results.NotFound()

).Produces<Fornecedor>(StatusCodes.Status200OK)
 .Produces(StatusCodes.Status404NotFound)
 .WithName("GetFornecedorPorId")
 .WithTags("Fornecedor");

app.MapPost("/fornecedor", async (MinimalContextDb contextDb, Fornecedor fornecedor) =>
{
    if (!MiniValidator.TryValidate(fornecedor, out var errors))
    {
        return Results.ValidationProblem(errors);
    }
    contextDb.fornecedors.Add(fornecedor);
    var result = await contextDb.SaveChangesAsync();

    return result > 0
    //? Results.Created($"/fornecedor/{fornecedor.Id}",fornecedor)
    ? Results.CreatedAtRoute("GetFornecedorPorId", new { id = fornecedor.Id }, fornecedor)
    : Results.BadRequest("Houve um problema ao Salvar o registo");

})  .ProducesValidationProblem()
    .Produces<Fornecedor>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PostFornecedor")
    .WithTags("Fornecedor");


app.MapPut("/fornecedor/{id}", async (Guid id, MinimalContextDb contextDb, Fornecedor fornecedor) =>
{
    var fornecedorBanco = await contextDb.fornecedors.AsNoTracking<Fornecedor>().FirstOrDefaultAsync(f=>f.Id==id);
    if (fornecedorBanco == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(fornecedor, out var errors))
    {
        return Results.ValidationProblem(errors);
    }
    contextDb.fornecedors.Update(fornecedor);
    var result = await contextDb.SaveChangesAsync();

    return result > 0 ? Results.NoContent() : Results.BadRequest("Houve um problema ao Salvar o registo");
}).ProducesValidationProblem()
   .Produces(StatusCodes.Status204NoContent)
   .Produces(StatusCodes.Status400BadRequest)
   .WithName("PutFornecedor")
   .WithTags("Fornecedor");

app.MapDelete("/fornecedor/{id}", async (Guid id, MinimalContextDb contextDb) =>
{
    var fornecedor = await contextDb.fornecedors.FindAsync(id);
    if (fornecedor == null) return Results.NotFound();

    contextDb.fornecedors.Remove(fornecedor);
    var result = await contextDb.SaveChangesAsync();

    return result > 0 ? Results.NoContent() : Results.BadRequest("Houve um problema ao Salvar o registo");
}).Produces(StatusCodes.Status400BadRequest)
   .Produces(StatusCodes.Status204NoContent)
   .Produces(StatusCodes.Status404NotFound)
   .WithName("DeleteFornecedor")
   .WithTags("Fornecedor");


app.Run();

