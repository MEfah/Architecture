using System.ComponentModel.DataAnnotations.Schema;

namespace PAPS4.Models
{
    [NotMapped]
    public class SurveyFilterOption
    {
        public SurveyProperty Property { get; set; }
        public FilterType FilterType { get; set; }
        public string FilterArgument { get; set; }
    }

    public enum SurveyProperty
    {
        Name, Description, Rating, Completions, Comments, Category
    }

    public enum FilterType
    {
        Range, Equals, OptionsList, StringContains
    }
}
