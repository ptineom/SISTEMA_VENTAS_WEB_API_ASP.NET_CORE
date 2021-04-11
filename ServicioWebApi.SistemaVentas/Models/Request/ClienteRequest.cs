using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class ClienteRequest
    {
        public string IdCliente { get; set; }
        public int IdTipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public bool FlgPersonaNatural { get; set; }
        public string RazonSocial { get; set; }
        public string Contacto { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Nombres { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string IdDepartamento { get; set; }
        public string IdProvincia { get; set; }
        public string IdDistrito { get; set; }
        public string Sexo { get; set; }
        public string Observacion { get; set; }
        public bool FlgInactivo { get; set; }

    }
}
