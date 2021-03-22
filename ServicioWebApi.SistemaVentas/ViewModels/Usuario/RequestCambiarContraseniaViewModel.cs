using Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Usuario
{
    public class CambiarContraseniaViewModel
    {
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        [Display(Name = "Usuario")]
        public string IdUsuario { get; set; }

        [Display(Name = "Contraseña actual")]
        public string ContraseniaActual { get; set; }

        [Required(ErrorMessage = "Debe de ingresar la {0}")]
        [StringLength(maximumLength: 32, ErrorMessage = Constantes.StringLengthMensaje, MinimumLength = 3)]
        [Display(Name = "Contraseña nueva")]
        public string ContraseniaNueva { get; set; }

        [Required(ErrorMessage = "Debe de {0}")]
        [StringLength(maximumLength: 32, ErrorMessage = Constantes.StringLengthMensaje, MinimumLength = 3)]
        [Display(Name = "Repetir contraseña nueva")]
        [Compare("ContraseniaNueva", ErrorMessage = "Debe de coincidir la {0} y {1}")]
        public string RepetirContraseniaNueva { get; set; }
    }
}
