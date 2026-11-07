using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Uber.BLL.ModelVM.Ride;

namespace MVCDay3.Controllers
{
    public class MapController : Controller
    {

        private readonly string orsApiKey = "eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6Ijc3MWY5NzUwNmI4ODQ3ZTBhZjllNjgxYjk2MjVlOTRmIiwiaCI6Im11cm11cjY0In0=";
        public IActionResult Temp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetRoute([FromBody] RouteRequest request)
        {
            string url = $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={orsApiKey}&start={request.StartLng},{request.StartLat}&end={request.EndLng},{request.EndLat}";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            var features = json["features"]?[0];
            if (features == null)
                return Json(new { error = "No route found" });

            var coordsArray = features["geometry"]?["coordinates"];
            var summary = features["properties"]?["summary"];

            if (coordsArray == null || summary == null)
                return Json(new { error = "No route data" });

            var latLngCoords = coordsArray.Select(coord => new double[] { (double)coord[1], (double)coord[0] }).ToList();

            double distanceMeters = (double)summary["distance"];
            double durationSeconds = (double)summary["duration"];

            return Json(new
            {
                coordinates = latLngCoords,
                distance = $"{(distanceMeters / 1000):0.00} km",
                time = $"{(durationSeconds / 60):0} mins"
            });
        }

     
    }
}