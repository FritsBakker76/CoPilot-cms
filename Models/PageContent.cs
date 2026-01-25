using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsModern.Models
{
    [Table("pagecontent")]
    public class PageContent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        public string Duration { get; set; }
        public string PictureText { get; set; }
        public string Type { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }
        [Column("created")]
        public DateTime Created { get; set; }

        // Navigation property
        public Page Page { get; set; }
    }
}