using Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SistemaVentas.WebApi.ViewModels.Usuario;

using System.Security.Cryptography;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using CapaNegocio;
using Entidades;
using SistemaVentas.WebApi.ViewModels;
using System.Text.Json;

namespace SistemaVentas.WebApi.Servicios.Seguridad
{
    public class TokenGenerator
    {
        private IConfiguration _configuracion { get; }
        public TokenGenerator(IConfiguration configuracion)
        {
            this._configuracion = configuracion;
        }

        #region "Métodos públicos"
        public TokensViewModel getTokens(UsuarioViewModel modelo)
        {
            //Generación de jwt
            TokenGenerator tokenGenerator = new TokenGenerator(_configuracion);
            string token = tokenGenerator.generateJWT(modelo);

            //Generacíon del refreshToken
            string refreshToken = tokenGenerator.generateRefreshToken();
            //Serializamos los claims
            string jsonClaims = JsonSerializer.Serialize(new
            {
                ID_USUARIO = modelo.idUsuario,
                NOM_USUARIO = modelo.nomUsuario,
                NOM_ROL = modelo.nomRol,
                ID_SUCURSAL = modelo.idSucursal,
                NOM_SUCURSAL = modelo.nomSucursal,
                FLG_CTRL_TOTAL = modelo.flgCtrlTotal
            });

            BrRefreshToken brRefreshToken = new BrRefreshToken();
            ResultadoOperacion resultado = new ResultadoOperacion();
            //Grabamos el refreshToken en la BD.
            resultado = brRefreshToken.grabarRefreshToken(new REFRESH_TOKEN()
            {
                ID_REFRESH_TOKEN = HashHelper.GetHash256(refreshToken),
                ID_USUARIO_TOKEN = modelo.idUsuario,
                TIEMPO_EXPIRACION_MINUTOS = Convert.ToInt32(_configuracion.GetSection("APP_SETTINGS:JWT:REFRESH_TOKEN_EXPIRE_MINUTES").Value),
                FEC_CREACION_UTC = DateTime.UtcNow,
                IP_ADDRESS = modelo.ipAddress,
                ID_USUARIO_REGISTRO = modelo.idUsuario,
                JSON_CLAIMS = jsonClaims
            });

            return new TokensViewModel() { accessToken = token, refreshToken = refreshToken };
        }

        #endregion

        #region "Métodos privados"
        private string generateJWT(UsuarioViewModel usuario)
        {
            var audienceToken = _configuracion.GetSection("APP_SETTINGS:JWT:JWT_AUDIENCE_TOKEN").Value;
            var issuerToken = _configuracion.GetSection("APP_SETTINGS:JWT:JWT_ISSUER_TOKEN").Value;
            var expireTime = _configuracion.GetSection("APP_SETTINGS:JWT:JWT_EXPIRE_MINUTES").Value;

            //Codifica el secretKey en hash256. Nota: el secretKey deberá tener como minimo 16 caracateres.
            var secretKey = HashHelper.GetHash256(_configuracion.GetSection("APP_SETTINGS:JWT:JWT_SECRET_KEY").Value);

            // CREAMOS EL HEADER //
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var _Header = new JwtHeader(signingCredentials);

            // CREAMOS EL PAYLOAD //
            IEnumerable<Claim> _Claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, usuario.nomRol),
                new Claim(ClaimTypes.Name, usuario.idUsuario),
                new Claim("idUsuario", usuario.idUsuario),
                new Claim("fullName", usuario.nomUsuario),
                new Claim("idSucursal", usuario.idSucursal),
                new Claim("nomSucursal", usuario.nomSucursal),
                new Claim("flgCtrlTotal", usuario.flgCtrlTotal.ToString())
            };
            var _Payload = new JwtPayload(
                    issuer: issuerToken,
                    audience: audienceToken,
                    claims: _Claims,
                    notBefore: DateTime.UtcNow,
                    // Expira
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime))
                );

            // GENERAMOS EL TOKEN //
            var _Token = new JwtSecurityToken(
                    _Header,
                    _Payload
                );

            return new JwtSecurityTokenHandler().WriteToken(_Token);
        }

        private string generateRefreshToken()
        {
            var RandomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(RandomNumber);
                return Convert.ToBase64String(RandomNumber);
            }
        }
        #endregion
    }
}
