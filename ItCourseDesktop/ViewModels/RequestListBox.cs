using System;

namespace ItCourseDesktop.Models;

public class RequestListBox
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Course { get; set; }
    public DateTime RequestDate { get; set; }
    public string Comment { get; set; }
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Patronymic { get; set; }
    public string UserPhone { get; set; } = null!;

    public string UserEmail { get; set; } = null!;
    
    public string Status { get; set; }
}