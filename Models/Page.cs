
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsModern.Models
{
    public class Page
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Column("menu_item")]
        public string MenuItem { get; set; }
        [Column("google_title")]
        public string GoogleTitle { get; set; }
        [Column("google_description")]
        public string GoogleDescription { get; set; }
        [Column("banner_path")]
        public string BannerPath { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
