using System;
using System.Collections.Generic;

namespace Lesson10Areas.Models;

public partial class Tag
{
    public string Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
