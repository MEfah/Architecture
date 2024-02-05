using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAPS4.Models
{
    public class Survey
    {
        [SwaggerSchema(ReadOnly = true)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<SurveyQuestion>? Questions { get; set; }
    }
}
