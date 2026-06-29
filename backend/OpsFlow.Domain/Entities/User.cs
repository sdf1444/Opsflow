using OpsFlow.Domain.Common;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Domain.Entities;

public class User : BaseEntity
{
  public string Name { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public string PasswordHash { get; set; } = string.Empty;

  public UserRole Role { get; set; }

  public ICollection<Request> RequestsCreated { get; set; }
      = new List<Request>();

  public ICollection<Request> RequestsAssigned { get; set; }
      = new List<Request>();

  public ICollection<RequestComment> Comments { get; set; }
      = new List<RequestComment>();

  public ICollection<AuditLog> AuditLogs { get; set; }
      = new List<AuditLog>();
}