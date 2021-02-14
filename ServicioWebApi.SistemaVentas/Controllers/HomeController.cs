using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("[action]")]
        [AllowAnonymous]
        public IActionResult getData()
        {
            List<string> nombres = new List<string>() {
            "Hector", "junior", "toro", "valencia" };
            return Ok(nombres);
        }
    }

}
