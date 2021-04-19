using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.ViewModel
{
    public class UsuarioModel
    {
        public string IdUsuario { get; set; }
        public string NomUsuario { get; set; }
        public string NomRol { get; set; }
        public string IdSucursal { get; set; }
        public string NomSucursal { get; set; }
        public bool FlgCtrlTotal { get; set; }
        public string IpAddress { get; set; }
        public string NameIdentifier { get; set; }
    }
}
