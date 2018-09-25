using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Workforce.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Slack handle")]
        [MaxLength(20)]
        public string SlackHandle { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select a Cohort")]
        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }

        [Display(Name = "Instructor Name")]
        public string FullName
        {
            get
            {
                return $"{this.FirstName} {this.LastName}";
            }
        }
    }
}
