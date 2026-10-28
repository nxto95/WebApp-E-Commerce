
using WebApplication.Database;

namespace WebApplication.Models;

public class BaseEntity : IAuditable, ISoftDelete
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}
