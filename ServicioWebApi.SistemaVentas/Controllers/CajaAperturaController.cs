using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class CajaAperturaController : ControllerBase
    {
        private IConfiguration _configuration = null;
        private IHttpContextAccessor _accessor;
        private IResultadoOperacion _resultado = null;
        private BrCajaApertura _brCajaApertura = null;
        private string _idSucursal;
        private string _idUsuario;

        public CajaAperturaController(IResultadoOperacion resultado, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _brCajaApertura = new BrCajaApertura(_configuration);
            _accessor = accessor;

            UsuarioViewModel usuario = new Session(_accessor).GetUserLogged();
            _idUsuario = usuario.IdUsuario;
            _idSucursal = usuario.IdSucursal;
        }

        [HttpGet("GetData")]
        public async Task<IActionResult> GetDataAsync()
        {
            List<MONEDA> listaMoneda = null;
            List<CAJA> listaCaja = null;
            _resultado = await Task.Run(() => _brCajaApertura.GetData(_idSucursal, _idUsuario, ref listaMoneda, ref listaCaja));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (listaMoneda == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar la moneda en el sistema.", Status = "Error" });

            if (listaCaja == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las cajas en el sistema.", Status = "Error" });


            //Moneda local
            var monedaLocal = listaMoneda.Where(x => x.FLG_LOCAL == true).Select(y => new
            {
                IdMoneda = y.ID_MONEDA,
                NomMoneda = y.NOM_MONEDA,
                SgnMoneda = y.SGN_MONEDA
            }).FirstOrDefault();


            //Resultado final 
            _resultado = new ResultadoOperacion();
            _resultado.SetResultado(true, new
            {
                ListaCaja = listaCaja.Select(x => new
                {
                    IdCaja = x.ID_CAJA,
                    NomCaja = ViewHelper.CapitalizeFirstLetter(x.NOM_CAJA)
                }).ToList<object>(),
                MonedaLocal = monedaLocal
            });

            return Ok(_resultado);
        }
    }
}
