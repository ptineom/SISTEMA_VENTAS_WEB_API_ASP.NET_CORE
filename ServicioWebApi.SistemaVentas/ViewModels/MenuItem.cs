using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels
{
    public class MenuItem
    {
        public string label { get; set; }
        public string icon { get; set; }
        public string route { get; set; }
        public bool flgRaiz { get; set; }
        public List<MenuItem> children { get; set; }
        public List<object> breadcrumbs { get; set; }
        public bool flgHome { get; set; }
    }
}
