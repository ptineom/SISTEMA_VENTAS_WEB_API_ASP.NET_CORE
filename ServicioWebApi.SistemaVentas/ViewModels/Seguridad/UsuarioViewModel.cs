using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Seguridad
{
    public class UsuarioViewModel
    {
        public string idUsuario { get; set; }
        public string nomUsuario { get; set; }
        public string nomRol { get; set; }
        public string idSucursal { get; set; }
        public string nomSucursal { get; set; }
        public bool flgCtrlTotal { get; set; }
        public string ipAddress { get; set; }
    }
}
