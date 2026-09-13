using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;  
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CoffeeNChill.Functions.Functions
{
    public class DocumentFunctions
    {
        private readonly BlobContainerClient _containerClient;

        public DocumentFunctions()
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? "UseDevelopmentStorage=true";
            var containerName = Environment.GetEnvironmentVariable("BlobContainerName")
                ?? "staffdocuments";

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists();
        }

        /// <summary>
        /// Uploads a staff document to Azure Blob Storage using multipart/form-data.
        /// POST /api/documents/upload
        /// </summary>
        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> UploadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents/upload")]
            HttpRequestData req)
        {
            try
            {
                if (!req.Headers.TryGetValues("Content-Type", out var contentTypes) ||
                    !MediaTypeHeaderValue.TryParse(contentTypes.FirstOrDefault(), out var mediaType) ||
                    string.IsNullOrEmpty(mediaType.Boundary.Value))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Missing or invalid Content-Type header.");
                    return badResponse;
                }

                var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;
                var reader = new MultipartReader(boundary, req.Body);
                MultipartSection section;
                MultipartSection fileSection = null;

                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var cd) &&
                        cd.DispositionType.Equals("form-data") &&
                        !string.IsNullOrEmpty(cd.FileName.Value))
                    {
                        fileSection = section;
                        break;
                    }
                }

                if (fileSection == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("No file uploaded.");
                    return badResponse;
                }

                var contentDisposition = ContentDispositionHeaderValue.Parse(fileSection.ContentDisposition);
                var fileName = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value;
                var contentType = fileSection.ContentType ?? "application/octet-stream";

                // Read file into memory to get length, then upload
                using var ms = new MemoryStream();
                await fileSection.Body.CopyToAsync(ms);
                ms.Position = 0;

                var blobClient = _containerClient.GetBlobClient(fileName);
                await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = contentType });

                // get uploaded size from memory stream or blob properties
                var size = ms.Length;

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(new
                {
                    fileName = fileName,
                    size = size,
                    contentType = contentType,
                    uploadedAt = DateTime.UtcNow,
                    uri = blobClient.Uri.ToString()
                });
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        /// <summary>
        /// Lists all staff documents in the blob container with metadata.
        /// GET /api/documents
        /// </summary>
        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> ListStaffDocuments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents")]
            HttpRequestData req)
        {
            try
            {
                var documents = new List<object>();

                await foreach (var blobItem in _containerClient.GetBlobsAsync())
                {
                    var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                    var properties = await blobClient.GetPropertiesAsync();

                    documents.Add(new
                    {
                        name = blobItem.Name,
                        size = properties.Value.ContentLength,
                        lastModified = properties.Value.LastModified,
                        contentType = properties.Value.ContentType,
                        uri = blobClient.Uri.ToString()
                    });
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(documents);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        /// <summary>
        /// Downloads a staff document as a stream from Blob Storage.
        /// GET /api/documents/download/{fileName}
        /// </summary>
        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> DownloadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/download/{fileName}")]
            HttpRequestData req, string fileName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(fileName);

                // Check if blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"File '{fileName}' not found.");
                    return notFound;
                }

                // Get blob properties for content type
                var properties = await blobClient.GetPropertiesAsync();

                // Download the blob
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", properties.Value.ContentType ?? "application/octet-stream");
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                var blobStream = await blobClient.OpenReadAsync();
                await blobStream.CopyToAsync(response.Body);

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