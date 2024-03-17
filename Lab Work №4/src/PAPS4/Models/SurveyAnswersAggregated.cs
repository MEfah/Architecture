using System.ComponentModel.DataAnnotations.Schema;

namespace PAPS4.Models
{
    [NotMapped]
    public class SurveyAnswersAggregated
    {
        public string PublicationId { get; set; }
        public ICollection<QuestionAnswersAggregated> AnswerDistribution { get; set; }
    }

    [NotMapped]
    public class QuestionAnswersAggregated
    {
        public int QuestionId { get; set; }
        public ICollection<AnswerCategoryDistribution> AnswerCategories { get; set; }
    }

    [NotMapped]
    public class AnswerCategoryDistribution
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }
}
