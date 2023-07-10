using System;

namespace ItCourseDesktop.ViewModels;

public class UserListBox
{
    public int Id { get; set; }
    
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    public string Firsname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Patronomic { get; set; }
    
    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public string RoleName { get; set; }
    
    public DateTime DateCreated { get; set; }
}