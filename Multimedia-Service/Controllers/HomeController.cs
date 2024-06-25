using MultimediaService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MultimediaService.Context;
using System.Security.Claims;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MultimediaService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserContext _userContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, UserContext userContext, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userContext = userContext;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var token = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var url = Url.Action("List", "User", new { }, Request.Scheme);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var userListJson = await response.Content.ReadAsStringAsync();
                    var userList = JsonConvert.DeserializeObject<List<User>>(userListJson);

                    // Remove the current logged-in user from the list
                    var currentUser = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var filteredUserList = userList.Where(u => u.Id.ToString() != currentUser).ToList();
                    ViewBag.UserID = currentUser;
                    return View(new Home
                    {
                        Users = filteredUserList,
                        HttpContextAccessor = _httpContextAccessor
                    });
                }
                else
                {
                    return Redirect("/Home/Login");
                }
            }
            catch (Exception)
            {
                return Redirect("/Home/Login");
            }
        }
        public IActionResult Login()
        {
            return View(new LoginModel());
        }
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        public IActionResult Video()
        {
            return View();
        }

        public IActionResult CaptureImage()
        {
            return View();
        }

        public IActionResult Audio()
        {
            return View();
        }
        public async Task<IActionResult> AccountAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var token = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var url = Url.Action("Current", "User", new { }, Request.Scheme);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userJson);
                return View(new AccountModel(user));
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
