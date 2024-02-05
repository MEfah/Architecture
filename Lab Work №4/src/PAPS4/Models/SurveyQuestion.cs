using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PAPS4.Models
{
    public class SurveyQuestion
    {
        public string PublicationId { get; set; }
        [ForeignKey(nameof(PublicationId))]
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public Survey? Survey { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }

        public SurveyQuestionType QuestionType { get; set; }
        public ICollection<string>? Options { get; set; }
    }

    public enum SurveyQuestionType
    {
        SingleSelect, MultiSelect, TextInput, MapOptions
    }
}
