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
using System.Security.Cryptography;
using CapaNegocio;
using Entidades;
using System.Text.Json;
using ServicioWebApi.SistemaVentas.Models.ViewModel;

namespace ServicioWebApi.SistemaVentas.Servicios.Seguridad
{
    public class TokenGenerator
    {
        private IConfiguration _configuracion = null;
        public TokenGenerator(IConfiguration configuracion)
        {
            this._configuracion = configuracion;
        }

        #region "Métodos públicos"
        public TokenModel GetTokens(UsuarioModel modelo)
        {
            //Generación de jwt
            TokenGenerator tokenGenerator = new TokenGenerator(_configuracion);
            string token = tokenGenerator.GenerateJWT(modelo);

            //Generacíon del refreshToken
            string refreshToken = tokenGenerator.GenerateRefreshToken();
            //Serializamos los claims
            string jsonClaims = JsonSerializer.Serialize(new
            {
                ID_USUARIO = modelo.IdUsuario,
                NOM_USUARIO = modelo.NomUsuario,
                NOM_ROL = modelo.NomRol,
                ID_SUCURSAL = modelo.IdSucursal,
                NOM_SUCURSAL = modelo.NomSucursal,
                FLG_CTRL_TOTAL = modelo.FlgCtrlTotal
            });

            BrRefreshToken brRefreshToken = new BrRefreshToken(_configuracion);

            //Grabamos el refreshToken en la BD.
            ResultadoOperacion resultado = brRefreshToken.Register(new REFRESH_TOKEN()
            {
                ID_REFRESH_TOKEN = HashHelper.GetHash256(refreshToken),
                ID_USUARIO_TOKEN = modelo.IdUsuario,
                TIEMPO_EXPIRACION_MINUTOS = Convert.ToInt32(_configuracion.GetSection("AppSettings:Jwt:RefreshTokenExpireMinutes").Value),
                FEC_CREACION_UTC = DateTime.UtcNow,
                IP_ADDRESS = modelo.IpAddress,
                ID_USUARIO_REGISTRO = modelo.IdUsuario,
                JSON_CLAIMS = jsonClaims
            });

            return new TokenModel() { AccessToken = token, RefreshToken = refreshToken };
        }

        #endregion

        #region "Métodos privados"
        private string GenerateJWT(UsuarioModel usuario)
        {
            var audienceToken = _configuracion.GetSection("AppSettings:Jwt:AudienceToken").Value;
            var issuerToken = _configuracion.GetSection("AppSettings:Jwt:IssuerToken").Value;
            var expireTime = _configuracion.GetSection("AppSettings:Jwt:ExpireMinutes").Value;

            //Codifica el secretKey en hash256. Nota: el secretKey deberá tener como minimo 16 caracateres.
            var secretKey = HashHelper.GetHash256(_configuracion.GetSection("AppSettings:Jwt:SecretKey").Value);

            // CREAMOS EL HEADER //
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var _Header = new JwtHeader(signingCredentials);

            // CREAMOS EL PAYLOAD //
            IEnumerable<Claim> _Claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, usuario.NomRol),
                new Claim(ClaimTypes.Name, usuario.IdUsuario),
                new Claim("IdUsuario", usuario.IdUsuario),
                new Claim("FullName", usuario.NomUsuario),
                new Claim("IdSucursal", usuario.IdSucursal),
                new Claim("NomSucursal", usuario.NomSucursal),
                new Claim("FlgCtrlTotal", usuario.FlgCtrlTotal.ToString())
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

        private string GenerateRefreshToken()
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
