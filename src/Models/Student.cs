using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Workforce.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [Display(Name="First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name="Last name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name="Slack handle")]
        [MaxLength(20)]
        public string SlackHandle { get; set; }

        [Required]
        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }

        [Display(Name="Student Name")]
        public string FullName {
            get {
                return $"{FirstName} {LastName}";
            }
        }

        public List<Exercise> AssignedExercises { get; set; } = new List<Exercise>();
    }

}