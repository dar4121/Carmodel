using Microsoft.AspNetCore.Mvc;

namespace Carmodel.Controllers;

public class CarModelsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}