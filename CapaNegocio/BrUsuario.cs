using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDao;
using Entidades;
using System.Data;
using Helper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace CapaNegocio
{
    public class BrUsuario
    {
        private DaoUsuario _dao = null;
        private ResultadoOperacion _resultado = null;
        private Conexion _conexion = null;
        public BrUsuario()
        {
            _dao = new DaoUsuario();
            _resultado = new ResultadoOperacion();
        }
        public ResultadoOperacion ValidateUser(string idUsuario, string clave, bool noValidar = false)
        {
            USUARIO modelo = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    _resultado.SetResultado(false, "Usuario y/o contraseña incorrectas");
                    modelo = _dao.ValidateUser(con, idUsuario, clave, noValidar);
                    _resultado.SetResultado(true, "", modelo);
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message);
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }
        
        public ResultadoOperacion GetAll()
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.GetAll(con);
                    if (lista != null)
                    {
                        _resultado.Data = lista;// lista.ToList<Object>();
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion Register(USUARIO oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Register(con, trx, oModelo);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion Delete(string idUsuario, string idUsuarioRegistro)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Delete(con, trx, idUsuario, idUsuarioRegistro);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeEliminadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion GetById(string idUsuario)
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    USUARIO modelo = _dao.GetById(con, idUsuario);
                    if (modelo != null)
                    {
                        _resultado.Data = modelo;
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion GetUsersActivated()
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    List<USUARIO> lista = _dao.GetUsersActivated(con);
                    if (lista != null)
                    {
                        _resultado.Data = lista;// lista.ToList<Object>();
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion ChangePassword(USUARIO oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.ChangePassword(con, trx, oModelo);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion SaveTokenRecoveryPassword(USUARIO oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.SaveTokenRecoveryPassword(con, trx, oModelo);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion RestorePassword(USUARIO oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.RestorePassword(con, trx, oModelo);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }

    }
}
