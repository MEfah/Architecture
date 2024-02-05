using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PAPS4.Models.DTOs
{
    public class SurveyAnswerDTO
    {
        [SwaggerSchema(ReadOnly = true)]
        public string? PublicationId { get; set; }
        [SwaggerSchema(ReadOnly = true)]
        public string? UserId { get; set; }

        public IEnumerable<QuestionAnswerDTO> QuestionAnswers { get; set; }
    }

    public class QuestionAnswerDTO
    {
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}
