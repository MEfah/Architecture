using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PAPS4.Models;

namespace PAPS4.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }


        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // использование Fluent API
            base.OnModelCreating(modelBuilder);
            // TODO!!!
            var surveyQuestionJSONConverter = new ValueConverter<ICollection<string>, string>(v => string.Join("|", v), v => v.Split(new[] {'|'}));
            var comparer = new ValueComparer<ICollection<string>>
            (
                (l, r) => (l == null ? "" : string.Join("|", l)) == (r == null ? "" : string.Join("|", r)),
                v => v == null ? 0 : string.Join("|", v).GetHashCode(),
                v => string.Join("|", v).Split(new[] { '|' })
            );

            var surveyQuestionPropBuilder = modelBuilder.Entity<SurveyQuestion>()
                   .Property(nameof(SurveyQuestion.Options));
            surveyQuestionPropBuilder.HasConversion(surveyQuestionJSONConverter);
            surveyQuestionPropBuilder.Metadata.SetValueConverter(surveyQuestionJSONConverter);
            surveyQuestionPropBuilder.Metadata.SetValueComparer(comparer);

            modelBuilder.Entity<SurveyQuestion>().HasKey(nameof(SurveyQuestion.Id), nameof(SurveyQuestion.PublicationId));
            modelBuilder.Entity<SurveyAnswer>().HasKey(nameof(SurveyAnswer.PublicationId), nameof(SurveyAnswer.UserId), nameof(SurveyAnswer.QuestionId));

            modelBuilder.Entity<Survey>().HasMany(s => s.Questions).WithOne(q => q.Survey).HasForeignKey(s => s.PublicationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}

/*{
    "id": "string",
  "name": "Survey1",
  "description": "Description for survey 1",
  "questions": [
    {
        "publicationId": "string",
      "id": 0,
      "text": "Question 1",
      "survey": "string",
      "questionType": 0,
      "options": [
        "Q1 Option 1", "Q1 Option 2"
      ],

    },{
        "publicationId": "string",
      "id": 1,
      "survey": "string",
      "text": "Question 2",
      "questionType": 0,
      "options": [
        "Q2 Option 1", "Q2 Option 2"
      ],

    }
  ]
}*/

/*{
    "id": "c986906e-d083-4cda-8856-9e93ca591084",
  "name": "Survey1",
  "description": "Description for survey 1",
  "questions": [
    {
        "publicationId": "c986906e-d083-4cda-8856-9e93ca591084",
      "id": 0,
      "text": "Question 1",
      "questionType": 0,
      "options": [
        "Q1 Option 1",
        "Q1 Option 2"
      ]
    },
    {
        "publicationId": "c986906e-d083-4cda-8856-9e93ca591084",
      "id": 1,
      "text": "Question 2",
      "questionType": 0,
      "options": [
        "Q2 Option 1",
        "Q2 Option 2"
      ]
    }
  ]
}*/
/*{
    "id": "989f7c34-31d0-445e-a0e9-2d9f52b2157c",
  "name": "Survey3",
  "description": "Description for survey 3",
  "questions": [
    {
        "publicationId": "989f7c34-31d0-445e-a0e9-2d9f52b2157c",
      "id": 0,
      "text": "Question 1",
      "questionType": 0,
      "options": [
        "Q1 Option 1",
        "Q1 Option 2",
        "Q1 Option 3",
        "Q1 Option 4"
      ]
    },
    {
        "publicationId": "989f7c34-31d0-445e-a0e9-2d9f52b2157c",
      "id": 1,
      "text": "Question 2",
      "questionType": 0,
      "options": [
        "Q2 Option 1",
        "Q2 Option 2"
      ]
    },
    {
        "publicationId": "989f7c34-31d0-445e-a0e9-2d9f52b2157c",
      "id": 2,
      "text": "Question 3",
      "questionType": 0,
      "options": [
        "Q3 Option 1",
        "Q3 Option 2"
      ]
    }
  ]
}*/