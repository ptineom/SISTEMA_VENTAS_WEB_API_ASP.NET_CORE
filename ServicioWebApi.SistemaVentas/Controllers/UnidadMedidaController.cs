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
    public class UnidadMedidaController : ControllerBase
    {
        private IResultadoOperacion _resultado;
        private BrUnidadMedida _brUm;
        public UnidadMedidaController()
        {
            _resultado = new ResultadoOperacion();
            _brUm = new BrUnidadMedida();
        }

        [HttpGet("listaUmPorFamilia/{idGrupo}/{idFamilia}")]
        public async Task<IActionResult> listaUmPorFamilia(string idGrupo, string idFamilia)
        {

            _resultado = await Task.Run(() => _brUm.listaUmPorFamilia(idGrupo, idFamilia));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las unidades de medidas.", Status = "Error" });

            _resultado.data = ((List<UNIDAD_MEDIDA>)_resultado.data).Select(x => new
            {
                idUm = x.ID_UM,
                nomUm = x.NOM_UM
            }).ToList<object>(); ;

            return Ok(_resultado);
        }
    }
}
