using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Session
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [Required]
    public virtual User User { get; set; } = null!;
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndDate { get; set; }
    
    public string Device { get; set; } = "Web";
    
    public string Ip { get; set; } = "127.0.0.1";
}