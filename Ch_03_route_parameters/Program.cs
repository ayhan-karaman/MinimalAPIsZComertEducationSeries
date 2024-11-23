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
app.MapGet("/employees", () => new Employee().GetAllEployees());
app.MapGet("/employees/search", (string q) => Employee.Search(q));
app.MapGet("/employees/{id:int}", (int id) => new Employee().GetFindByIdEmployee(id));
app.MapPost("/employees/add", (Employee employee) => Employee.CreateEmployee(employee));

app.Run();

class Employee
{
    public int Id { get; set; }
    public String FullName { get; set; }
    public Decimal Salary { get; set; }

    private static List<Employee> Employees = new List<Employee>()
    {
         new Employee(){ Id = 1, FullName ="Ömer Kaya", Salary=95_000},
         new Employee(){ Id = 2, FullName ="Çetin Karataş", Salary=72_000},
         new Employee(){ Id = 3, FullName ="Elvan Karaman", Salary=87_000},
    };

    public List<Employee> GetAllEployees() => Employees;
    public Employee? GetFindByIdEmployee(int id) =>  Employees.SingleOrDefault(e => e.Id.Equals(id));
    public static List<Employee>? Search(string q) 
    {
        return Employees.Where(e => e.FullName != null && e.FullName.ToLower().Contains(q)).ToList();
    }
    
    public static void CreateEmployee(Employee employee) => Employees.Add(employee);


} 

