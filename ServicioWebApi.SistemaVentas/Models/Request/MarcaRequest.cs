using Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class MarcaRequest
    {
        public int IdMarca { get; set; }

        [Required(ErrorMessage = Constantes.RequiredMensaje)]
        [Display(Name = "Marca")]
        public string NomMarca { get; set; }
    }
}
