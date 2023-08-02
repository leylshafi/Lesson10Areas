using System;
using System.Collections.Generic;

namespace Lesson10Areas.Models;

public partial class Product
{
    public string Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string CategoryId { get; set; }

    public DateTime CreatedTime { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
