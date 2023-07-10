using System;

namespace ItCourseAPI.DTO;

public class RequestDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime RequestDate { get; set; }

    public string? Comment { get; set; }
    
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string UserPhone { get; set; } = null!;

    public string UserEmail { get; set; } = null!;
    
    public int StatusId { get; set; }
    
    
}