using System;

namespace ItCourseDesktop.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Patronymic { get; set; }
    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }
    
    public DateTime CreatedDate { get; set; }
}