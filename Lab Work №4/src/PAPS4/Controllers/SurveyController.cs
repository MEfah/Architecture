using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAPS4.Database;
using PAPS4.Models;
using PAPS4.Models.DTOs;
using System.Diagnostics;

namespace PAPS4.Controllers
{
    [ApiController]
    [Route("surveys")]
    public class SurveyController : Controller
    {
        private ApplicationContext ApplicationContext { get; }
        public SurveyController(ApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;
        }

        /// <summary>
        /// Получить список опросов
        /// </summary>
        /// <remarks>
        /// Возвращает список опросов в соответствии с указанным размером и отступом. В будущем в теле запроса также будут передаваться опции сортировки и фильтрации
        /// </remarks>
        /// <param name="count">Количество опросов в возвращаемом списке</param>
        /// <param name="offset">Отступ возвращаемого списка опросов</param>
        /// <returns cref="">Список опросов</returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="400">Неверно указаны параметры запроса</response>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<Survey>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        public IActionResult GetSurveys(
            [FromQuery] int count = 20, 
            [FromQuery] int offset = 0)
        {
            return Json(ApplicationContext.Surveys.Include(s => s.Questions).Skip(offset).Take(count).ToList());
        }

        /// <summary>
        /// Создать и опубликовать опрос
        /// </summary>
        /// <remarks>
        /// Размещает новый опрос в системе. Для опроса генерируется GUID
        /// </remarks>
        /// <param name="survey">Опрос</param>
        /// <returns>Возвращает опрос со сгенерированным GUID</returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="400">Ошибка в теле запроса</response>
        [HttpPost]
        [ProducesResponseType(typeof(Survey), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        public IActionResult AddSurvey(Survey survey)
        {
            Guid guid = Guid.NewGuid();
            survey.Id = guid.ToString();

            if (survey.Questions != null)
                foreach(var question in survey.Questions)
                    question.PublicationId = survey.Id;

            ApplicationContext.Surveys.Add(survey);
            ApplicationContext.SaveChanges();

            var result = Json(survey);
            result.StatusCode = 200;

            return result;
        }

        /// <summary>
        /// Получить опрос
        /// </summary>
        /// <remarks>
        /// Возвращает опрос с указанным идентификатором
        /// </remarks>
        /// <param name="publicationId">GUID опроса</param>
        /// <returns>Опрос</returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="404">Запрос не найден</response>

        [HttpGet]
        [Route("{publicationId:minlength(36)}")]
        [ProducesResponseType(typeof(Survey), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetSurvey(string publicationId)
        {
            Survey? survey = ApplicationContext.Surveys.Where(s => s.Id == publicationId).Include(s => s.Questions).FirstOrDefault();

            if (survey == null)
                return NotFound("Опрос не найден");

            var result = Json(survey);
            result.StatusCode = 200;

            return result;
        }

        /// <summary>
        /// Удалить опрос
        /// </summary>
        /// <remarks>
        /// Удаляет опрос с указанным GUID
        /// </remarks>
        /// <param name="publicationId">GUID опроса</param>
        /// <returns></returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="404">Опрос не найден</response>
        [HttpDelete]
        [Route("{publicationId:minlength(36)}")]
        [ProducesResponseType(typeof(Survey), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult DeleteSurvey(string publicationId)
        {
            Survey? survey = ApplicationContext.Surveys.Find(publicationId);

            if (survey == null)
                return NotFound("Опрос не найден");

            ApplicationContext.Entry(survey).State = EntityState.Deleted;
            ApplicationContext.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Добавить ответ на опрос
        /// </summary>
        /// Добавляет ответ на опрос с указанным GUID от пользователя с указанным идентификатором
        /// <param name="publicationId">GUID опроса</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="surveyAnswer">Ответ на опрос</param>
        /// <returns></returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="400">Некорректно указаны параметры или тело запроса. Количество вопросов в ответе не совпадает с опросом</response>
        /// <response code="404">Опрос или пользователь не найдены</response>
        /// <response code="409">Ответ на указанный опрос от указанного пользователя уже существует</response>
        [HttpPost]
        [Route("{publicationId:minlength(36)}/answers/{userId:minlength(5)}")]
        [ProducesResponseType(typeof(SurveyAnswerDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        public IActionResult AddSurveyAnswer(
            [FromRoute] string publicationId, 
            [FromRoute] string userId, 
            [FromBody] SurveyAnswerDTO surveyAnswer)
        {
            Survey? survey = ApplicationContext.Surveys.Include(s => s.Questions).SingleOrDefault(s => s.Id == publicationId);
            if (survey == null)
                return NotFound("Опрос не был найден");

            bool userExists = ApplicationContext.Users.Any(u => u.Id == userId);
            if (!userExists)
                return NotFound("Пользователь не был найден");

            bool answerExists = ApplicationContext.SurveyAnswers.Any(sa => sa.UserId == userId && sa.PublicationId == publicationId);
            if (answerExists)
                return Conflict("Ответ пользователя на опрос уже существует");

            if (survey.Questions == null || surveyAnswer.QuestionAnswers.Count() != survey.Questions.Count)
                return BadRequest("Количество вопросов в опросе и ответе не совпадает");

            List<SurveyAnswer> answers = new();
            foreach(var questionAnswer in surveyAnswer.QuestionAnswers)
            {
                SurveyAnswer answer = new()
                {
                    PublicationId = publicationId,
                    UserId = userId,
                    QuestionId = questionAnswer.QuestionId,
                    Answer = questionAnswer.Answer
                };
                answers.Add(answer);
            }

            ApplicationContext.SurveyAnswers.AddRange(answers);
            ApplicationContext.SaveChanges();

            surveyAnswer.PublicationId = publicationId;
            surveyAnswer.UserId = userId;
            var result = Json(surveyAnswer);
            result.StatusCode = 200;

            return result;
        }

        /// <summary>
        /// Изменить ответ на опрос
        /// </summary>
        /// Изменяет ответ на опрос с указанным GUID от пользователя с указанным идентификатором
        /// <param name="publicationId">GUID опроса</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="surveyAnswer">Новый ответ на опрос</param>
        /// <returns></returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="400">Некорректно указаны параметры или тело запроса. Количество вопросов в ответе не совпадает с опросом</response>
        /// <response code="404">Опрос или пользователь не найдены</response>
        [HttpPut]
        [Route("{publicationId:minlength(36)}/answers/{userId:minlength(5)}")]
        [ProducesResponseType(typeof(SurveyAnswerDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult ChangeSurveyAnswer(
            [FromRoute] string publicationId,
            [FromRoute] string userId,
            [FromBody] SurveyAnswerDTO surveyAnswer)
        {
            Survey? survey = ApplicationContext.Surveys.Where(s => s.Id == publicationId).Include(s => s.Questions).FirstOrDefault();

            if (survey == null)
                return NotFound("Опрос не найден");

            if (survey.Questions!.Count != surveyAnswer.QuestionAnswers.Count())
                return BadRequest("Количество вопросов в ответе и опросе не совпадает");

            var exists = ApplicationContext.SurveyAnswers.Any(s => s.PublicationId == publicationId && s.UserId == userId);

            if (!exists)
                return NotFound("Ответ пользователя на указанный опрос не найден");

            var userAnswer = ApplicationContext.SurveyAnswers.Where(s => s.PublicationId == publicationId && s.UserId == userId);
            foreach (var questionAnswer in surveyAnswer.QuestionAnswers)
            {
                SurveyAnswer answer = new()
                {
                    PublicationId = publicationId,
                    UserId = userId,
                    QuestionId = questionAnswer.QuestionId,
                    Answer = questionAnswer.Answer
                };
                var existing = ApplicationContext.SurveyAnswers.SingleOrDefault(s => s.PublicationId == publicationId && s.UserId == userId && s.QuestionId == questionAnswer.QuestionId);
                
                if(existing != null)
                {
                    existing.Answer = questionAnswer.Answer;
                }
                    
            }
            
            ApplicationContext.SaveChanges();

            surveyAnswer.PublicationId = publicationId;
            surveyAnswer.UserId = userId;
            var result = Json(surveyAnswer);
            result.StatusCode = 200;

            return result;
        }

        /// <summary>
        /// Получить ответ на опрос
        /// </summary>
        /// Возвращает ответ на опрос с указанным GUID от пользователя с указанным идентификатором
        /// <param name="publicationId">GUID опроса</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Ответ на опрос</returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="404">Ответ пользователя на опрос не найден</response>
        [HttpGet]
        [Route("{publicationId:minlength(36)}/answers/{userId:minlength(5)}")]
        [ProducesResponseType(typeof(SurveyAnswerDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult GetSurveyAnswerOfUser(string publicationId, string userId)
        {
            var existing = ApplicationContext.SurveyAnswers.Where(s => s.PublicationId == publicationId && s.UserId == userId).ToList();

            if (existing == null || existing.Count == 0)
            {
                return NotFound("Ответ пользователя на указанный опрос не найден");
            }

            List<QuestionAnswerDTO> questionAnswers = new();

            foreach(var answer in existing)
            {
                questionAnswers.Add(new()
                {
                    QuestionId = answer.QuestionId,
                    Answer = answer.Answer
                });
            }


            SurveyAnswerDTO dto = new()
            {
                PublicationId = publicationId,
                UserId = userId,
                QuestionAnswers = questionAnswers
            };

            var result = Json(dto);
            result.StatusCode = 200;

            return result;
        }

        /// <summary>
        /// Удалить ответ на опрос
        /// </summary>
        /// Удаляет ответ на опрос с указанным GUID от пользователя с указанным идентификатором
        /// <param name="publicationId">GUID опроса</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        /// <response code="200">Запрос успешно выполнен</response>
        /// <response code="404">Ответ пользователя на опрос не найден</response>
        [HttpDelete]
        [Route("{publicationId:minlength(36)}/answers/{userId:minlength(5)}")]
        [ProducesResponseType(typeof(SurveyAnswerDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult RemoveSurveyAnswer(string publicationId, string userId)
        {
            bool exists = ApplicationContext.SurveyAnswers.Any(s => s.PublicationId == publicationId && s.UserId == userId);
            if (!exists)
                return NotFound("Ответ на опрос не найден");

            ApplicationContext.SurveyAnswers.Where(s => s.PublicationId == publicationId && s.UserId == userId).ExecuteDelete();
            ApplicationContext.SaveChanges();

            
            return Ok();
        }
    }
}
