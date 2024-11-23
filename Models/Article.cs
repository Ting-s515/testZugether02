using System;
using System.Collections.Generic;

namespace Zugether.Models;

public partial class Article
{
    public short article_id { get; set; }

    public string content { get; set; } = null!;

    public byte[]? photo { get; set; }

    public DateTime created_at { get; set; }
}
