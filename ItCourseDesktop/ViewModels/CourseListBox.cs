using Avalonia.Media.Imaging;
using Newtonsoft.Json;

namespace ItCourseDesktop.ViewModels;

public class CourseListBox
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImagePath { get; set; }

    public int DurationHours { get; set; }

    public string Level { get; set; }

    [JsonIgnore] public Bitmap Image { get; set; }
}