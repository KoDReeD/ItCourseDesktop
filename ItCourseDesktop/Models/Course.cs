namespace ItCourseDesktop.Models;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImagePath { get; set; }

    public int DurationHours { get; set; }

    public int LevelId { get; set; }
}