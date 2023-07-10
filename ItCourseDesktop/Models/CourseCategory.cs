using System;
using System.Collections.Generic;

namespace ItCourseAPI.DbModels;

public partial class CourseCategory
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int CategoryId { get; set; }
}
