using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class ReaperturarCajaRequest
    {
        public string IdCaja { get; set; }
        public string IdUsuario { get; set; }
        public int Correlativo { get; set; }
    }
}
