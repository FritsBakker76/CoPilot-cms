using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsModern.Models
{
    [Table("websitesettings")]
    public class WebsiteSettings
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(500)]
        public string LogoPath { get; set; }

        [MaxLength(200)]
        public string SiteTitle { get; set; }

        [MaxLength(20)]
        public string HeaderBg { get; set; }

        [MaxLength(20)]
        public string HeaderTextColor { get; set; }

        [MaxLength(20)]
        public string MenuBg { get; set; }

        [MaxLength(20)]
        public string MenuTextColor { get; set; }

        [MaxLength(20)]
        public string MenuAlignment { get; set; }

        [MaxLength(20)]
        public string SiteBg { get; set; }

        [MaxLength(20)]
        public string SiteTextColor { get; set; }

        [MaxLength(20)]
        public string FooterBg { get; set; }

        [MaxLength(20)]
        public string FooterTextColor { get; set; }

        public int? FontPageTitle { get; set; }
        public int? FontAlineaTitle { get; set; }
        public int? FontWebsiteText { get; set; }
        public int? FontSlideshowFooter { get; set; }

        public string FooterContact { get; set; }
        public string FooterOpeningHours { get; set; }
        public string FooterSocial { get; set; }
    }
}