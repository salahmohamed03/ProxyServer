using Microsoft.AspNetCore.Mvc;
using ProxyServer.Models;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace ProxyServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly HttpClient _client;
        public HomeController(ILogger<HomeController> logger , HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public IActionResult Index(ResponseViewModel? response = null)
        {
            
            return View(response);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult GetResponse(
            Method method
            ,string uri
            ,string header
            ,string body)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Parse(method.ToString()), uri);
                if (!string.IsNullOrEmpty(header))
                {
                    using JsonDocument headerJson = JsonDocument.Parse(header);
                    JsonElement headerElement = headerJson.RootElement;
                    foreach (var item in headerElement.EnumerateObject())
                    {
                        request.Headers.Add(item.Name, item.Value.GetString());
                    }
                }
                if(method != Method.Get && !string.IsNullOrEmpty(body))
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.SendAsync(request).Result;
                var status = response.StatusCode.ToString();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                if (responseBody != null && !responseBody.Equals(""))
                {
                    using JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                    responseBody = JsonSerializer.Serialize(jsonDocument.RootElement, new JsonSerializerOptions { WriteIndented = true });

                }
                return View("Index", new ResponseViewModel { Response = responseBody , statusCode = status});
            }
            catch (Exception ex)
            {
                return View("Index", new ResponseViewModel { Response = ex.Message });
            }
        }
    }
}
public enum Method
{
    Get,
    Post,
    Put,
    Delete
}