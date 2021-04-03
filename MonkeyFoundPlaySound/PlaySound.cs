using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MonkeyFoundPlaySound
{
    public static class PlaySound
    {
        public static bool currentsoundplaystatus = false;
        [FunctionName("PlaySound")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (bool.TryParse(req.Query["playlionroar"], out bool res))
            {
                currentsoundplaystatus = res;
                return new OkObjectResult(currentsoundplaystatus);
            }
            else
            {
                return new BadRequestObjectResult($"Invalid argument for {req.Query["playlionroar"]} ");
            }
        }
    }
}

