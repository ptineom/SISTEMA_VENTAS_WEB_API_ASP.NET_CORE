using CapaNegocio;
using Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServicioWebApi.SistemaVentas.Models.Request;
using ServicioWebApi.SistemaVentas.Models.ViewModel;
using ServicioWebApi.SistemaVentas.Servicios.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private IConfiguration _configuration = null;
        private IHttpContextAccessor _accessor;
        private IResultadoOperacion _resultado = null;
        private BrCliente _brCliente = null;
        private string _idSucursal;
        private string _idUsuario;

        public ClienteController(IResultadoOperacion resultado, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _brCliente = new BrCliente(_configuration);
            _accessor = accessor;

            UsuarioModel usuario = new Session(_accessor).GetUserLogged();
            _idUsuario = usuario.IdUsuario;
            _idSucursal = usuario.IdSucursal;
        }

        [HttpGet("GetByDocument/{idTipoDocumento?}/{nroDocumento?}")]
        public async Task<IActionResult> GetByDocumentAsync(int idTipoDocumento, string nroDocumento)
        {
            _resultado = await Task.Run(() => _brCliente.GetByDocument(idTipoDocumento, nroDocumento));

            if (!_resultado.Resultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });
            }

            if (_resultado.Data == null)
            {
                return NotFound(new { Message = "No se encontraron datos", Status = "Eror" });
            };

            CLIENTE cliente = (CLIENTE)_resultado.Data;
            _resultado.Data = new
            {
                IdCliente = cliente.ID_CLIENTE,
                NroDocumento = cliente.NRO_DOCUMENTO,
                IdTipoDocumento = cliente.ID_TIPO_DOCUMENTO,
                NomCliente = cliente.NOM_CLIENTE,
                DirCliente = cliente.DIR_CLIENTE
            };
            return Ok(_resultado);
        }

        [HttpGet("GetAllByFilters/{tipoFiltro?}/{filtro?}/{flgConInactivos?}")]
        public async Task<IActionResult> GetAllByFilters(string tipoFiltro, string filtro, bool flgConInactivos = false)
        {
            _resultado = await Task.Run(() => _brCliente.GetAllByFilters(tipoFiltro, filtro, flgConInactivos));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (_resultado.Data == null)
                return NotFound(new { Message = "No se encontraron datos", Status = "Eror" });

            List<CLIENTE> lista = (List<CLIENTE>)_resultado.Data;


            _resultado.Data = lista.Select(x => new
            {
                IdCliente = x.ID_CLIENTE,
                NomCliente = x.NOM_CLIENTE,
                NomTipoDocumento = x.ABREVIATURA,
                NroDocumento = x.NRO_DOCUMENTO,
                DirCliente = x.DIR_CLIENTE,
                IdTipoDocumento = x.ID_TIPO_DOCUMENTO
            });

            return Ok(_resultado);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(ClienteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            _resultado = await Task.Run(() => _brCliente.Register(new CLIENTE()
            {
                ID_TIPO_DOCUMENTO= request.IdTipoDocumento,
                NRO_DOCUMENTO = request.NroDocumento,
                FLG_PERSONA_NATURAL = request.FlgPersonaNatural,
                NOM_CLIENTE = request.RazonSocial,
                CONTACTO = request.Contacto,
                APELLIDO_PATERNO = request.ApellidoPaterno,
                APELLIDO_MATERNO = request.ApellidoMaterno,
                NOMBRES = request.Nombres,
                SEXO = request.Sexo,
                EMAIL_CLIENTE = request.Email,
                TEL_CLIENTE = request.Telefono,
                DIR_CLIENTE = request.Direccion,
                ID_UBIGEO = string.IsNullOrEmpty(request.IdDistrito) ? "-1" : request.IdDistrito,
                ID_USUARIO_REGISTRO = _idUsuario,
                OBS_CLIENTE = request.Observacion,
                FLG_INACTIVO = request.FlgInactivo,
                ACCION = "INS"
            }));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            return Ok(_resultado);
        }
    }
}
