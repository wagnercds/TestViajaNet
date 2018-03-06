using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Text;

namespace testLog.Controllers
{
    /* Classe para salvar as informações do log*/
    public class LogInformation
    {
        public string userToken { get; set; }
        public string functionName { get; set; }
        public IDictionary<string, object> arguments { get; set; }
        public DateTime dateNavigation { get; set; }
    }

    /* Classe para enviar o log via RabbitMQ */
    public class Log : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LogInformation logInformation = new LogInformation()
            {
                userToken = "3EBD8B43-2CE4-41EA-AECA-A7C9434B6020", /* Token do usuário logado (em uma versão real deverá ser substituido pelo controle de login utilizado) */
                functionName = filterContext.ActionDescriptor.DisplayName,
                arguments = filterContext.ActionArguments,
                dateNavigation = DateTime.Now
            };
            ConnectionFactory connectionfactory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connection = connectionfactory.CreateConnection())
            using (IModel model = connection.CreateModel())
            {
                model.QueueDeclare("logNavigation", false, false, false, null);
                model.BasicPublish("", "logNavigation", null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(logInformation)));
            }
        }
    }
}