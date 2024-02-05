using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PAPS4.Models
{
    public class SurveyAnswer
    {
        public string PublicationId { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(PublicationId))]
        [JsonIgnore]
        public Survey? Survey { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User? User { get; set; }

        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}
