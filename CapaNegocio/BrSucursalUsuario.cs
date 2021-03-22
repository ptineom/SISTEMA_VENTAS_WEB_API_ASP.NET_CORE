using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDao;
using Entidades;
using System.Data.SqlClient;
using Helper;
namespace CapaNegocio
{
    public class BrSucursalUsuario
    {
        DaoSucursalUsuario dao = null;
        ResultadoOperacion oResultado = null;
        public BrSucursalUsuario()
        {
            dao = new DaoSucursalUsuario();
            oResultado = new ResultadoOperacion();
        }
        public ResultadoOperacion GetUsersBySucursalId(string idSucursal, ref List<SUCURSAL_USUARIO> listaSucUsu)
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    var lista = dao.GetUsersBySucursalId(con, idSucursal, ref listaSucUsu);
                 
                    oResultado.SetResultado(true, lista);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    oResultado.SetResultado(false, ex.Message);
                }
            }
            return oResultado;
        }

        public ResultadoOperacion Register(SUCURSAL_USUARIO oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    dao.Register(con, trx, oModelo);
                    oResultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    oResultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return oResultado;
        }

        public ResultadoOperacion Delete(string idSucursal, string idUsuario, string idUsuarioRegistro)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    dao.Delete(con, trx, idSucursal, idUsuario, idUsuarioRegistro);
                    oResultado.SetResultado(true, Helper.Constantes.sMensajeEliminadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    oResultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return oResultado;
        }

        public ResultadoOperacion GetSucursalesByUserId(string idUsuario)
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    var lista = dao.GetSucursalesByUserId(con, idUsuario);
                    if (lista != null)
                    {
                        oResultado.Data = lista.ToList<SUCURSAL>().Select(x => new
                        {
                           IdSucursal = x.ID_SUCURSAL,
                           NomSucursal = x.NOM_SUCURSAL
                        });
                    }
                    oResultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    oResultado.SetResultado(false, ex.Message);
                }
            }
            return oResultado;
        }
    }
}
