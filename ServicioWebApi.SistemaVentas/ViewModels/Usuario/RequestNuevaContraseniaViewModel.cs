using Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Usuario
{
    public class RequestNuevaContraseniaViewModel
    {
        [Display(Name = "Nueva contraseña")]
        [Required(ErrorMessage = "Debe de ingresar la {0}")]
        [StringLength(maximumLength: 32, ErrorMessage = Constantes.StringLengthMensaje, MinimumLength = 3)]
        public string NuevaContrasenia { get; set; }

        [Display(Name = "Confirmar nueva contraseña")]
        [Required(ErrorMessage = "Debe de ingresar la confirmación de la nueva contraseña")]
        [StringLength(maximumLength: 32, ErrorMessage = Constantes.StringLengthMensaje, MinimumLength = 3)]
        [Compare("NuevaContrasenia", ErrorMessage = "Deben de coincidir los campos {1} y {0}")]
        public string RepetirNuevaContrasenia { get; set; }

        [Required(ErrorMessage = "Debe de ingresar el token de recuperación de contraseña")]
        public string TokenRecuperacionPassword { get; set; }
    }
}
