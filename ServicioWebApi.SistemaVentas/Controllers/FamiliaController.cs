using CapaNegocio;
using Entidades;
using Helper;
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
    public class FamiliaController : ControllerBase
    {
        private IConfiguration _configuration = null;
        private IResultadoOperacion _resultado;
        private BrFamilia _brFamilia;
        public FamiliaController(IConfiguration configuration)
        {
            _configuration = configuration;
            _resultado = new ResultadoOperacion();
            _brFamilia = new BrFamilia(_configuration);
        }

        [HttpGet("GetAllByGroupIdHelper/{idGrupo}")]
        public async Task<IActionResult> GetAllByGroupIdHelperAsync(string idGrupo)
        {
            _resultado = await Task.Run(() => _brFamilia.GetAllByGroupIdHelper(idGrupo));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (_resultado.Data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las clasificaciones de familas.", Status = "Error" });

            List<object> familias = ((List<FAMILIA>)_resultado.Data).Select(x => new
            {
                IdFamilia = x.ID_FAMILIA,
                NomFamilia = x.NOM_FAMILIA 
            }).ToList<object>();

            _resultado.Data = familias;

            return Ok(_resultado);
        }
    }
}
