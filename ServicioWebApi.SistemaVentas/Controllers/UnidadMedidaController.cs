using CapaNegocio;
using Entidades;
using Helper;
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

        [HttpGet("GetAllByFamilyId/{idGrupo}/{idFamilia}")]
        public async Task<IActionResult> GetAllByFamilyIdAsync(string idGrupo, string idFamilia)
        {

            _resultado = await Task.Run(() => _brUm.GetAllByFamilyId(idGrupo, idFamilia));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (_resultado.Data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las unidades de medidas.", Status = "Error" });

            _resultado.Data = ((List<UNIDAD_MEDIDA>)_resultado.Data).Select(x => new
            {
                IdUm = x.ID_UM,
                NomUm = ViewHelper.capitalizeFirstLetter(x.NOM_UM)
            }).ToList<object>(); ;

            return Ok(_resultado);
        }
    }
}
