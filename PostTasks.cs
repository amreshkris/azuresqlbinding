using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace bdotnet
{
    public static class PostTasks
    {
        [FunctionName("PostTasks")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [Sql("dbo.ToDo",ConnectionStringSetting = "SqlConnectionString")] 
                IAsyncCollector<ToDoTask> toDoItems)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ToDoTask todoTask = JsonConvert.DeserializeObject<ToDoTask>(requestBody);

            todoTask.Id = Guid.NewGuid();
            todoTask.url =  Environment.GetEnvironmentVariable("ToDoUri")+"?id="+todoTask.Id.ToString();
            if (todoTask.completed == null)
            {
                todoTask.completed = false;
            }
            await toDoItems.AddAsync(todoTask);
            await toDoItems.FlushAsync();
            var toDoItemList = new List<ToDoTask> { todoTask };
            return new OkObjectResult(toDoItemList);
        }
    }
}
