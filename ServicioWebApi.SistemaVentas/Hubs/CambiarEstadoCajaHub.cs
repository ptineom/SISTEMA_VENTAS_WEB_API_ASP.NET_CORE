using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ServicioWebApi.SistemaVentas.Hubs
{
    public class CambiarEstadoCajaHub: Hub
    {
        public async Task UpdateStateBox(string idUsuario)
        {
            await Clients.User(idUsuario).SendAsync("actualizarEstadoCaja");
        }

        public override Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name;
            string xx = Context.UserIdentifier;
            return base.OnConnectedAsync();
        }
    }
}
