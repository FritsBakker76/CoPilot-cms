using System.ComponentModel.DataAnnotations.Schema;

namespace CmsModern.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        [Column("password_hash")]
        public string PasswordHash { get; set; }
        [Column("is_admin")]
        public bool IsAdmin { get; set; }
    }
}