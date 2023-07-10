using System;
using System.Collections.Generic;
using ItCourseDesktop.Models;

namespace ItCourseAPI.DbModels;

public partial class UserCourseRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime RequestDate { get; set; }

    public string? Comment { get; set; }

    public int StatusId { get; set; }

    public string UserPhone { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string UserFirstname { get; set; } = null!;

    public string UserLastname { get; set; } = null!;

    public string? UserPatronomic { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
