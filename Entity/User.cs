using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    
    [Required]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    
    public bool Active { get; set; } = true;
    
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}