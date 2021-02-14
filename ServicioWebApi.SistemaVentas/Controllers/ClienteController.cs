using CapaNegocio;
using Entidades;
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
    public class ClienteController : ControllerBase
    {
        private IResultadoOperacion _resultado { get; set; }
        private BrCliente _brCliente { get; set; }

        public ClienteController(IResultadoOperacion resultado)
        {
            this._resultado = resultado;
            this._brCliente = new BrCliente();
        }

        [HttpGet("obtenerClientePorDocumentoAsync/{idTipoDocumento?}/{nroDocumento?}")]
        public async Task<IActionResult> obtenerClientePorDocumentoAsync(int idTipoDocumento, string nroDocumento)
        {
            _resultado = await Task.Run(() => _brCliente.clientePorDocumento(idTipoDocumento, nroDocumento));

            if (!_resultado.bResultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });
            }

            if (_resultado.data == null)
            {
                return NotFound(new { Message = "No se encontraron datos", Status = "Eror" });
            };

            CLIENTE cliente = (CLIENTE)_resultado.data;
            _resultado.data = new
            {
                idCliente = cliente.ID_CLIENTE,
                nroDocumento = cliente.NRO_DOCUMENTO,
                idTipoDocumento = cliente.ID_TIPO_DOCUMENTO,
                nomCliente = cliente.NOM_CLIENTE,
                dirCliente = cliente.DIR_CLIENTE
            };
            return Ok(_resultado);
        }

        [HttpGet("listaClientes/{tipoFiltro?}/{filtro?}/{flgConInactivos?}")]
        public async Task<IActionResult> listaClientes(string tipoFiltro, string filtro, bool flgConInactivos = false)
        {
            _resultado = await Task.Run(() => _brCliente.listaClientes(tipoFiltro, filtro, flgConInactivos));

            if (!_resultado.bResultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });
            }

            if (_resultado.data == null)
            {
                return NotFound(new { Message = "No se encontraron datos", Status = "Eror" });
            };

            List<CLIENTE> lista = (List<CLIENTE>)_resultado.data;


            _resultado.data = lista.Select(x => new
            {
                idCliente = x.ID_CLIENTE,
                nomCliente = x.NOM_CLIENTE,
                nomTipoDocumento = x.ABREVIATURA,
                nroDocumento = x.NRO_DOCUMENTO,
                dirCliente = x.DIR_CLIENTE,
                idTipoDocumento = x.ID_TIPO_DOCUMENTO
            });

            return Ok(_resultado);
        }
    }
}
