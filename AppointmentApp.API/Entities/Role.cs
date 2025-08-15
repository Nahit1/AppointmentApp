namespace AppointmentApp.API.Entities;

public class Role
{
    public int Id { get; set; }              // 1=student, 2=coach, 3=admin
    public string Name { get; set; } = default!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}