using System.Collections.Generic;
using ItCourseDesktop.Models;

namespace ItCourseDesktop.ViewModels;

public class CourseAddVM
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    
    public string ImagePath { get; set; }
    public int Price { get; set; }
    public int Duration { get; set; }
    public Level SelectedLevel { get; set; }
    public List<Category> Categories { get; set; }
    public List<Tehnology> Tehnologies { get; set; }
}