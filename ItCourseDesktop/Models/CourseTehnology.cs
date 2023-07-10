using System;
using System.Collections.Generic;

namespace ItCourseAPI.DbModels;

public partial class CourseTehnology
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int TehnologyId { get; set; }
}
