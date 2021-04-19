using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class CajaAbiertaRequest
    {
        public string Accion { get; set; }
        public string IdCaja { get; set; }
        public int Correlativo { get; set; }
        public decimal MontoApertura { get; set; }
        public string FechaApertura { get; set; }
        public decimal MontoTotal { get; set; }
        public string FechaCierre { get; set; }
        public string IdMoneda { get; set; }
        public bool FlgReaperturado { get; set; }
        public int Item { get; set; }
        public bool FlgCierreDiferido { get; set; }
    }
}
