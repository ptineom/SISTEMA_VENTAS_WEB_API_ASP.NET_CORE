using CapaNegocio;
using Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebApi.Seguridad;
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
        private BrMarca _brMarca = null;
        private IResultadoOperacion _resultado = null;
        private string _idUsuario;
        private string _idSucursal;

        public MarcaController(IResultadoOperacion resultado)
        {
            _brMarca = new BrMarca();
            _resultado = resultado;

            UsuarioViewModel usuario = Session.obtenerUsuarioLogueadoStatic();
            _idUsuario = usuario.idUsuario;
            _idSucursal = usuario.idSucursal;
        }

        [HttpGet("listaMarca/{nomMarca}")]
        public async Task<IActionResult> listaMarca(string nomMarca)
        {
            _resultado = await Task.Run(() => _brMarca.listaMarcas(nomMarca));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos no encontrado.", Status = "Error" });

            _resultado.data = ((List<MARCA>)_resultado.data).Select(x => new
            {
                idMarca = x.ID_MARCA,
                nomMarca = x.NOM_MARCA
            });

            return Ok(_resultado);
        }

        [HttpPost("grabarMarca")]
        public async Task<IActionResult> grabarMarca(MARCA oModelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            oModelo.ID_USUARIO_REGISTRO = _idUsuario;
            _resultado = await Task.Run(() => _brMarca.grabarMarca(oModelo));

            return Ok(_resultado);
        }
    }
}
