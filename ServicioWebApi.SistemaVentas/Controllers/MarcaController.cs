using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServicioWebApi.SistemaVentas.ViewModels;
using SistemaVentas.WebApi.Servicios.Seguridad;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcaController : ControllerBase
    {
        private IConfiguration _configuration = null;
        private BrMarca _brMarca = null;
        private IResultadoOperacion _resultado = null;
        private string _idUsuario;
        private string _idSucursal;
        public IHttpContextAccessor _accessor = null;

        public MarcaController(IConfiguration configuration, IResultadoOperacion resultado, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _brMarca = new BrMarca(_configuration);
            _resultado = resultado;
            _accessor = accessor;

            UsuarioViewModel usuario = new Session(_accessor).GetUserLogged();
            _idUsuario = usuario.IdUsuario;
            _idSucursal = usuario.IdSucursal;
        }

        [HttpGet("GetAllByDescription/{nomMarca}")]
        public async Task<IActionResult> GetAllByDescriptionAsync(string nomMarca)
        {
            _resultado = await Task.Run(() => _brMarca.GetAllByDescription(nomMarca));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (_resultado.Data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos no encontrado.", Status = "Error" });

            _resultado.Data = ((List<MARCA>)_resultado.Data).Select(x => new
            {
                IdMarca = x.ID_MARCA,
                NomMarca = ViewHelper.capitalizeAll(x.NOM_MARCA)
            });

            return Ok(_resultado);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RequestMarca request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            _resultado = await Task.Run(() => _brMarca.Register(new MARCA(){
                NOM_MARCA = request.NomMarca,
                ID_USUARIO_REGISTRO = _idUsuario,
                ACCION = "INS"
            }));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            return Ok(_resultado);
        }
    }
}
