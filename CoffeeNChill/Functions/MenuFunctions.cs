using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Functions
{
    public class MenuFunctions
    {
        private const string MenuTableName = "MenuItems";
        private readonly TableClient _tableClient;

        public MenuFunctions()
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? "UseDevelopmentStorage=true";
            _tableClient = new TableClient(connectionString, MenuTableName);
            _tableClient.CreateIfNotExists();
        }

        // 1. CREATE - POST /api/menu
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> CreateMenuItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "menu")] HttpRequestData req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var newItem = JsonSerializer.Deserialize<MenuItemEntity>(body);

                if (newItem == null ||
                    string.IsNullOrWhiteSpace(newItem.PartitionKey) ||
                    string.IsNullOrWhiteSpace(newItem.RowKey))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("PartitionKey (category) and RowKey (id) are required.");
                    return badResponse;
                }

                await _tableClient.UpsertEntityAsync(newItem);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(newItem);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // 2. READ ALL - GET /api/menu
        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> GetAllMenuItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "menu")] HttpRequestData req)
        {
            try
            {
                var items = new List<MenuItemEntity>();
                await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>())
                {
                    items.Add(item);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(items);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // 3. READ BY CATEGORY - GET /api/menu/category/{category}
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> GetMenuItemsByCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "menu/category/{category}")]
            HttpRequestData req, string category)
        {
            try
            {
                var items = new List<MenuItemEntity>();
                await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>(
                    filter: $"PartitionKey eq '{category}'"))
                {
                    items.Add(item);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(items);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // 4. UPDATE - PUT /api/menu/{category}/{id}
        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> UpdateMenuItem(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "menu/{category}/{id}")]
            HttpRequestData req, string category, string id)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var updateData = JsonSerializer.Deserialize<MenuItemEntity>(body);

                var existing = await _tableClient.GetEntityIfExistsAsync<MenuItemEntity>(category, id);

                if (!existing.HasValue)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"Item '{id}' in category '{category}' not found.");
                    return notFound;
                }

                var item = existing.Value;

                if (updateData.Price > 0)
                    item.Price = updateData.Price;

                item.IsAvailable = updateData.IsAvailable;

                if (!string.IsNullOrWhiteSpace(updateData.Name))
                    item.Name = updateData.Name;

                if (!string.IsNullOrWhiteSpace(updateData.Description))
                    item.Description = updateData.Description;

                await _tableClient.UpsertEntityAsync(item);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(item);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // 5. DELETE - DELETE /api/menu/{category}/{id}
        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> DeleteMenuItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "menu/{category}/{id}")]
            HttpRequestData req, string category, string id)
        {
            try
            {
                var existing = await _tableClient.GetEntityIfExistsAsync<MenuItemEntity>(category, id);

                if (!existing.HasValue)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"Item '{id}' in category '{category}' not found.");
                    return notFound;
                }

                await _tableClient.DeleteEntityAsync(category, id);

                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}