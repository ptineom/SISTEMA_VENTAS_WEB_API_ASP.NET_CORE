using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SucursalUsuarioController : ControllerBase
    {
        private IConfiguration _configuration { get; }
        private IResultadoOperacion _resultado { get; set; }
        private BrSucursalUsuario oBrSucursalUsuario = null;

        public SucursalUsuarioController(IConfiguration configuration, IResultadoOperacion resultado)
        {
            _configuration = configuration;
            _resultado = resultado;
            oBrSucursalUsuario = new BrSucursalUsuario();
        }

        [HttpGet("listaSucursalPorUsuarioAsync/{idUsuario?}")]
        [AllowAnonymous]
        public async Task<IActionResult> listaSucursalPorUsuarioAsync(string idUsuario)
        {
            //Lista de sucursales por usuario.
            _resultado = await Task.Run(() => oBrSucursalUsuario.listaSucursalPorUsuario(idUsuario));
            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            return Ok(_resultado);
        }
    }
}
