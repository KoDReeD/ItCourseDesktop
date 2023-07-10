using System.Collections.Generic;
using ItCourseAPI.DbModels;
using ItCourseDesktop.Models;

namespace ItCourseAPI.DTO;

public class CourseAddDTO
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; }

    public double Price { get; set; }

    public string? ImagePath { get; set; }

    public int DurationHours { get; set; }

    public int LevelId { get; set; }
    public List<Category> Categories { get; set; }
    public List<Tehnology> Tehnologies { get; set; }
}