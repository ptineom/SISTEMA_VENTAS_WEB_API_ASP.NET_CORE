using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class UsuarioSucursalRequest
    {
        [Required(ErrorMessage ="Debe ingresar el {0}")]
        public string IdSucursal { get; set; }
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string NomSucursal { get; set; }
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        [Display(Name = "usuario")]
        public string IdUsuario { get; set; }
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        public string Password { get; set; }
    }

    public class CambiarSucursalRequest
    {
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string IdSucursal { get; set; }
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string NomSucursal { get; set; }
    }
}
