using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using CapaDao;
using System.Data.SqlClient;
using Helper;
using Microsoft.Extensions.Configuration;

namespace CapaNegocio
{
    public class BrCliente
    {
        private IConfiguration _configuration = null;
        private DaoCliente _dao = null;
        private ResultadoOperacion _resultado = null;
        private Conexion _conexion = null;

        public BrCliente(IConfiguration configuration)
        {
            _dao = new DaoCliente();
            _resultado = new ResultadoOperacion();
            _configuration = configuration;
            _conexion = new Conexion(_configuration);
        }

        public ResultadoOperacion GetAllByFilters(string tipoFiltro, string filtro, bool flgConInactivos)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.GetAllByFilters(con, tipoFiltro, filtro, flgConInactivos);

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
        public ResultadoOperacion GetById(string idCliente)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    CLIENTE modelo = _dao.GetById(con, idCliente);
                    if (modelo != null)
                    {
                        _resultado.SetResultado(true, "", (Object)modelo);
                    }
                    else
                    {
                        _resultado.SetResultado(false, Constantes.sMensajeNohayRegistro);
                    }
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion Register(CLIENTE oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Register(con, trx, oModelo);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk, oModelo.ID_CLIENTE.ToString());
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
        public ResultadoOperacion Delete(string idCliente, string idUsuario)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Delete(con, trx, idCliente, idUsuario);
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
        public ResultadoOperacion GetByDocument(int idTipoDocumento, string nroDocumento)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    CLIENTE modelo = _dao.GetByDocument(con, idTipoDocumento, nroDocumento);
                    _resultado.SetResultado(true, modelo);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
    }
}
