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
    public class FamiliaController : ControllerBase
    {
        private IResultadoOperacion _resultado;
        private BrFamilia _brFamilia;
        public FamiliaController()
        {
            _resultado = new ResultadoOperacion();
            _brFamilia = new BrFamilia();
        }

        [HttpGet("cboFamilias/{idGrupo}")]
        public async Task<IActionResult> cboFamilias(string idGrupo)
        {
            _resultado = await Task.Run(() => _brFamilia.cboFamilia(idGrupo));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las clasificaciones de familas.", Status = "Error" });

            List<object> familias = ((List<FAMILIA>)_resultado.data).Select(x => new
            {
                idFamilia = x.ID_FAMILIA,
                nomFamilia = x.NOM_FAMILIA
            }).ToList<object>();

            _resultado.data = familias;

            return Ok(_resultado);
        }
    }
}
