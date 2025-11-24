using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Swagger (Generador de documentación API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configurar la Base de Datos en Memoria
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

// 3. Activar Swagger SIEMPRE (incluso en producción para Render)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// --- ENDPOINTS DE CLIMA ---
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast"); 
// NOTA: Aquí YA NO ESTÁ ".WithOpenApi()" para evitar el error 500

// --- ENDPOINTS DE TAREAS (Base de Datos) ---

// GET: Ver todas las tareas
app.MapGet("/todoitems", async (TodoDb db) => 
    await db.Todos.ToListAsync());

// GET: Ver tareas completadas
app.MapGet("/todoitems/complete", async (TodoDb db) => 
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

// POST: Crear tarea
app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

// GET: Ver tarea por ID
app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

// PUT: Actualizar tarea
app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE: Borrar tarea
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();

// --- MODELOS ---

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }
    public DbSet<Todo> Todos => Set<Todo>();
}