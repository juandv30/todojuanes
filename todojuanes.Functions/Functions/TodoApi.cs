using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using todojuanes.Common.Models;
using todojuanes.Common.Responses;
using todojuanes.Functions.Entities;

namespace todojuanes.Functions.Functions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new todo");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "The Request must have a TaskDescription."

                });
            }

            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                isCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = "New todo stored in table";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
                  [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
                  string id,
                  ILogger log)
        {
            log.LogInformation($"Update for todo: {id}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            //Validate todo id
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {

                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "Todo not found."

                });

            }

            // Update todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.isCompleted = todo.isCompleted;
            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;
            }


            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todoTable.ExecuteAsync(addOperation);

            string message = $"Todo: {id}, update in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(GetAllTodos))]
        public static async Task<IActionResult> GetAllTodos(
                 [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
                 [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
                 ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all todos";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todos
            });
        }
        [FunctionName(nameof(GetTodoById))]
        public static IActionResult GetTodoById(
                     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
                     [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
                     string id,
                     ILogger log)
        {
            log.LogInformation($"Get todo by id: {id}, received.");

            if (todoEntity == null)
            {

                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "Todo not found."

                });

            }

            string message = $"Todo: {todoEntity.RowKey}, retrieved";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(DeleteTodo))]
        public static async Task<IActionResult> DeleteTodo(
                           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
                           [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
                           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
                           string id,
                           ILogger log)
        {
            log.LogInformation($"Delete todo: {id}, received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    Message = "Todo not found."
                });
            }

            await todoTable.ExecuteAsync(TableOperation.Delete(todoEntity));
            string message = $"Todo: {todoEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        public static Task CreateTodo()
        {
            throw new NotImplementedException();
        }
    }
}
