using CapaDao;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Hosting;//nugget Microsoft.AspNetCore.Hosting.Abstractions
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CapaNegocio
{
    public class BrEmpresa
    {
        private DaoEmpresa _dao = null;
        private ResultadoOperacion _resultado = null;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;

        public BrEmpresa(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _dao = new DaoEmpresa();
            _resultado = new ResultadoOperacion();
            _configuration = configuration;
            _environment = environment;
        }

        public BrEmpresa()
        {
            _dao = new DaoEmpresa();
            _resultado = new ResultadoOperacion();
        }

        public ResultadoOperacion GetByFilters(string idSucursal, bool flgMostrarSucursales, bool flgSinFotos = false)
        {
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    EMPRESA modelo = _dao.GetByFilters(con, idSucursal, flgMostrarSucursales);

                    if (modelo != null)
                    {
                        if (!flgSinFotos)
                        {
                            string dominioWebApi = Configuraciones.DOMINIO_WEB_API;
                            string uploadEmpresa = Configuraciones.UPLOAD_EMPRESA;
                            
                            byte[] binaryFoto = null;

                            if (!string.IsNullOrEmpty(modelo.FOTO1) && !string.IsNullOrEmpty(modelo.FOTO2))
                            {
                                binaryFoto = System.IO.File.ReadAllBytes(Path.Combine(_environment.WebRootPath, uploadEmpresa, modelo.FOTO2));
                                modelo.FOTO_B64 = Convert.ToBase64String(binaryFoto);
                                modelo.FOTO1 = Path.Combine(dominioWebApi, uploadEmpresa, modelo.FOTO1);
                                modelo.FOTO2 = Path.Combine(dominioWebApi, uploadEmpresa, modelo.FOTO2);
                            }
                            else if (!string.IsNullOrEmpty(modelo.FOTO1) && string.IsNullOrEmpty(modelo.FOTO2))
                            {
                                binaryFoto = System.IO.File.ReadAllBytes(Path.Combine(_environment.WebRootPath, uploadEmpresa, modelo.FOTO1));
                                modelo.FOTO_B64 = Convert.ToBase64String(binaryFoto);
                                modelo.FOTO1 = Path.Combine(dominioWebApi, uploadEmpresa, modelo.FOTO1);
                                modelo.FOTO2 = Path.Combine(dominioWebApi, uploadEmpresa, modelo.FOTO1);
                            }
                        }

                        if (flgMostrarSucursales)
                        {
                            modelo.sucursales.ForEach(x =>
                            {
                                if (!string.IsNullOrEmpty(x.JSON_TELEFONOS))
                                {
                                    var telefonos = JsonSerializer.Deserialize<List<TELEFONO>>(x.JSON_TELEFONOS).ToList();
                                    var movil = telefonos.Where(y => y.TIPO == "CELULAR").Select(u => u.NUMERO).ToList<string>();
                                    var casa = telefonos.Where(y => y.TIPO == "CASA").Select(u => u.NUMERO).ToList<string>();
                                    var cadMovil = movil.Count > 0 ? "Celular: " + string.Join(',', movil) : "";
                                    var cadCasa = casa.Count > 0 ? "Telf.:" + string.Join(',', casa) : "";

                                    x.TELEFONOS = $"{cadCasa} | {cadMovil}";
                                }
                            });
                        }
                    }

                    _resultado.SetResultado(true, "", modelo);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
        public ResultadoOperacion Register(EMPRESA oModelo, ref string idEmpresa, ref string xmlFotos)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(Conexion.sConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();

                    _dao.Register(con, trx, oModelo, ref idEmpresa, ref xmlFotos);

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
