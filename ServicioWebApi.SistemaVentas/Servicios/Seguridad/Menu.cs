using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Hosting;
using SistemaVentas.WebApi.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Servicios.Seguridad
{
    public class Menu
    {
        public IWebHostEnvironment _environment { get; set; }
        private ResultadoOperacion _resultado = null;
        public Menu(IWebHostEnvironment environment)
        {
            _environment = environment;
            _resultado = new ResultadoOperacion();
        }

        #region "Métodos públicos"
        /// <summary>
        ///  Lista de menu según el usuario.
        /// </summary>
        /// <param name="idUsuario">Usuario por el que se hará el filtrado del los menús</param>
        /// <returns></returns>
        public MenuItem obtenerMenuPorUsuario(string idUsuario)
        {
            BrAplicacion brAplicacion = new BrAplicacion();

            //Obtenemos la lista de menu según el usuario.
            _resultado = brAplicacion.listarMenuUsuario(idUsuario);

            if (!_resultado.bResultado)
                throw new Exception(_resultado.sMensaje);

            //Construímos el menú a requerimiento del cliente.
            MenuItem menuItem = null;
            if (_resultado.data != null)
            {
                menuItem = new MenuItem();
                List<APLICACION> listaGeneral = (List<APLICACION>)_resultado.data;

                //Raiz del arbol el cual dará inicio.
                APLICACION aplicacionRaiz = listaGeneral.FirstOrDefault(x => x.FLG_RAIZ);

                //Método recursivo que construirá el arbol de menus.
                setChildren(aplicacionRaiz, listaGeneral, menuItem);

                //Marcamos a los primeros hijos como raiz para la renderización en la vista.
                menuItem.children.ForEach((elem) => elem.flgRaiz = true);
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
        private void setChildren(APLICACION aplicacion, List<APLICACION> listaGeneral, MenuItem menuItem)
        {
            menuItem.label = aplicacion.NOM_APLICACION;
            menuItem.icon = aplicacion.ICON_SPA;
            menuItem.route = aplicacion.ROUTE_SPA;
            menuItem.flgHome = aplicacion.FLG_HOME;

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
                menuItem.breadcrumbs = breadCrums;
            }
            //Si tiene hijos ejeuta la recursividad
            var childs = listaGeneral.Where(x => x.ID_APLICACION_PADRE == aplicacion.ID_APLICACION).ToList();
            if (childs.Count > 0)
            {
                List<MenuItem> listaSubMenu = new List<MenuItem>();
                foreach (var child in childs)
                {
                    MenuItem subMenu = new MenuItem();
                    setChildren(child, listaGeneral, subMenu);
                    listaSubMenu.Add(subMenu);
                };
                menuItem.children = listaSubMenu;
            }
        }

        #endregion

    }
}
