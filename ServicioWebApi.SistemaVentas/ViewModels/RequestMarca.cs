using Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.ViewModels
{
    public class RequestMarca
    {
        public int idMarca { get; set; }

        [Required(ErrorMessage = Constantes.RequiredMensaje)]
        [Display(Name = "Marca")]
        public string nomMarca { get; set; }
    }
}
