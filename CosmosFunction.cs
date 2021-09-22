using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CosmosAssignment
{
    public static class CosmosFunction
    {
        [FunctionName("CosmosFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, 
            [CosmosDB(
            databaseName: "MusicDB", 
            collectionName: "songs",
            ConnectionStringSetting = "CosmosDbConnectionString")]
            IAsyncCollector<dynamic> DbSongs, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try 
            {
                //Fill with more desired variables below if you want to expand with more properties for a song
                string artist = req.Query["artist"];
                string title = req.Query["title"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject<Music>(requestBody);
                artist = artist ?? data?.artist;
                title = title ?? data?.title;

                if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
                {
                var newSong = new Music
                    {
                        //Generate random ID
                        Id = System.Guid.NewGuid().ToString(),
                        Artist = artist,
                        Title = title
                    };

                await DbSongs.AddAsync(newSong);
                return new OkObjectResult(newSong);
                }
                log.LogError($"You have to fill in artist and title in order to insert into the databse");
                return new BadRequestObjectResult("Invalid input values. You have to add an artist AND a song value/input.");
            }
            catch (Exception ex)
            {
                log.LogError($"Your item was not successfully inserted into the database. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
