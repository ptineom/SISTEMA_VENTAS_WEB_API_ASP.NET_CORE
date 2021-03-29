using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels
{
    public class MenuItem
    {
        public string Label { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public bool FlgRaiz { get; set; }
        public List<MenuItem> Children { get; set; }
        public List<object> Breadcrumbs { get; set; }
        public bool FlgHome { get; set; }
        public bool FlgRequiereAperturaCaja { get; set; }
    }
}
