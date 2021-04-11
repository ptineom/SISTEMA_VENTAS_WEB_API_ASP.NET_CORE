using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ServicioWebApi.SistemaVentas.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Servicios.Seguridad
{
    public class Menu
    {
        private IConfiguration _configuration = null;
        private IWebHostEnvironment _environment = null;
        private ResultadoOperacion _resultado = null;

        public Menu(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _configuration = configuration;
            _environment = environment;
            _resultado = new ResultadoOperacion();
        }

        #region "Métodos públicos"
        /// <summary>
        ///  Lista de menu según el usuario.
        /// </summary>
        /// <param name="idUsuario">Usuario por el que se hará el filtrado del los menús</param>
        /// <returns></returns>
        public MenuItemModel GetMenuByUserId(string idUsuario)
        {
            BrAplicacion brAplicacion = new BrAplicacion(_configuration);

            //Obtenemos la lista de menu según el usuario.
            _resultado = brAplicacion.GetMenuByUserId(idUsuario);

            if (!_resultado.Resultado)
                throw new Exception(_resultado.Mensaje);

            //Construímos el menú a requerimiento del cliente.
            MenuItemModel menuItem = null;
            if (_resultado.Data != null)
            {
                menuItem = new MenuItemModel();
                List<APLICACION> listaGeneral = (List<APLICACION>)_resultado.Data;

                //Raiz del arbol el cual dará inicio.
                APLICACION aplicacionRaiz = listaGeneral.FirstOrDefault(x => x.FLG_RAIZ);

                //Método recursivo que construirá el arbol de menus.
                setChildren(aplicacionRaiz, listaGeneral, menuItem);

                //Marcamos a los primeros hijos como raiz para la renderización en la vista.
                menuItem.Children.ForEach((elem) => elem.FlgRaiz = true);
            }
            return menuItem;
        }

        public string avatarB64(string archivo)
        {
            string directorio = string.Empty;
            string b64 = "";
            byte[] foto = null;

            if (!string.IsNullOrEmpty(archivo))
            {
                string contentRootPath = _environment.WebRootPath;
                directorio = Path.Combine(contentRootPath, Configuraciones.UPLOAD_EMPLEADOS, archivo);
                foto = System.IO.File.ReadAllBytes(directorio);
                b64 = Convert.ToBase64String(foto);
            }
            else
            {
                string webRootPath = _environment.WebRootPath;
                directorio = Path.Combine(webRootPath, "Imagenes", "avatar_notFound.png");
                foto = System.IO.File.ReadAllBytes(directorio);
                b64 = Convert.ToBase64String(foto);
            }
            return b64;
        }
        #endregion

        #region "Métodos privados"
        private void setChildren(APLICACION aplicacion, List<APLICACION> listaGeneral, MenuItemModel menuItem)
        {
            menuItem.Label = aplicacion.NOM_APLICACION;
            menuItem.Icon = aplicacion.ICON_SPA;
            menuItem.Route = aplicacion.ROUTE_SPA;
            menuItem.FlgHome = aplicacion.FLG_HOME;
            menuItem.FlgRequiereAperturaCaja = aplicacion.FLG_REQUIERE_APERTURA_CAJA;

            if (aplicacion.FLG_FORMULARIO && !string.IsNullOrEmpty(aplicacion.BREADCRUMS))
            {
                //COnstruimos el array de objetos breadcrums
                string[] arrBreadCrums = aplicacion.BREADCRUMS.Split('|');
                List<object> breadCrums = new List<object>();
                for (int i = 0; i < arrBreadCrums.Length; i++)
                {
                    var obj = new
                    {
                        text = arrBreadCrums[i],
                        disabled = (i == (arrBreadCrums.Length - 1)) ? false : true
                    };
                    breadCrums.Add(obj);
                }
                menuItem.Breadcrumbs = breadCrums;
            }
            //Si tiene hijos ejeuta la recursividad
            var childs = listaGeneral.Where(x => x.ID_APLICACION_PADRE == aplicacion.ID_APLICACION).ToList();
            if (childs.Count > 0)
            {
                List<MenuItemModel> listaSubMenu = new List<MenuItemModel>();
                foreach (var child in childs)
                {
                    MenuItemModel subMenu = new MenuItemModel();
                    setChildren(child, listaGeneral, subMenu);
                    listaSubMenu.Add(subMenu);
                };
                menuItem.Children = listaSubMenu;
            }
        }

        #endregion

    }
}
