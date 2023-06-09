using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Model;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }
        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }


        public string CreateToken(int userId)
        {
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString())
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        tokenKeyString != null ? tokenKeyString : ""
                    )
                );

            SigningCredentials credentials = new SigningCredentials(
                    tokenKey,
                    SecurityAlgorithms.HmacSha512Signature
                );

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);

        }

        public bool SetPassword(UserForRegistration userLogin)
        {

            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash(userLogin.Password, passwordSalt);
           string sql = @"EXEC spRegisterUser
            @FirstName = @FirstNameParam,
            @LastName = @LastNameParam,
            @EmailAddress = @EmailAddressParam,
            @PasswordHash = @PasswordHashParam,
            @PasswordSalt = @PasswordSaltParam";
            DynamicParameters sqlParamters = new DynamicParameters();
            sqlParamters.Add("@FirstNameParam", userLogin.FirstName, DbType.String);
            sqlParamters.Add("@LastNameParam", userLogin.LastName, DbType.String);
            sqlParamters.Add("@EmailAddressParam", userLogin.EmailAddress, DbType.String);
            sqlParamters.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParamters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);
            Console.WriteLine(sql);
            return _dapper.ExecuteSqlWithParameters(sql, sqlParamters);

        }


        public bool SetPasswordAdmin(AdminRegister adminRegister)
        {

            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash(adminRegister.Password, passwordSalt);
            string sql = @"EXEC spRegister
            @FirstName = @FirstNameParam,
            @LastName = @LastNameParam,
            @EmailAddress = @EmailAddressParam,
            @PasswordHash = @PasswordHashParam,
            @PasswordSalt = @PasswordSaltParam";
            DynamicParameters sqlParamters = new DynamicParameters();
            sqlParamters.Add("@FirstNameParam", adminRegister.FirstName, DbType.String);
            sqlParamters.Add("@LastNameParam", adminRegister.LastName, DbType.String);
            sqlParamters.Add("@EmailAddressParam", adminRegister.EmailAddress, DbType.String);
            sqlParamters.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParamters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);
            Console.WriteLine(sql);
            return _dapper.ExecuteSqlWithParameters(sql, sqlParamters);

        }


    }
}