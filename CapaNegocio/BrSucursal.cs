using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using CapaDao;
using Helper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CapaNegocio
{
    public class BrSucursal
    {
        DaoSucursal _dao = null;
        ResultadoOperacion _resultado = null;
        IConfiguration _configuration = null;
        Conexion _conexion = null;

        public BrSucursal(IConfiguration configuration)
        {
            _dao = new DaoSucursal();
            _resultado = new ResultadoOperacion();
            _configuration = configuration;
            _conexion = new Conexion(_configuration);
        }

        public ResultadoOperacion GetAll()
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.GetAll(con);
                  
                    _resultado.SetResultado(true, lista);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion Register(SUCURSAL oModelo)
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
        public void GetById(string idSucursal, ref List<UBIGEO> listaProvincia, ref List<UBIGEO> listaDistrito)
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    _dao.GetById(con, idSucursal, ref listaProvincia, ref listaDistrito);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                }
            }
        }
        public ResultadoOperacion Delete(string idSucursal, string idUsuario)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Delete(con, trx, idSucursal, idUsuario);
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
       
    }
}
