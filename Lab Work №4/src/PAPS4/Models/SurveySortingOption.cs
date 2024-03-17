using System.ComponentModel.DataAnnotations.Schema;

namespace PAPS4.Models
{
    [NotMapped]
    public class SurveySortingOption
    {
        public SurveyProperty Property { get; set; }
        public bool Ascending { get; set; }
    }
}
