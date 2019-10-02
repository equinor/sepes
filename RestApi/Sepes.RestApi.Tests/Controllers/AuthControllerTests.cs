using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Sepes.RestApi;
using Sepes.RestApi.Controller;
using System.Linq;
using System.Text;
using System;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Sepes.RestApi.Model;
using Microsoft.Extensions.Options;

namespace Sepes.RestApi.Tests.Controller
{
    public class AuthControllerTests
    {
        private static string key = "erfagidfi9yhjeropgbhsrietjjksdjgtjklenfgophirgdhtjfxpiogjh";
        [Fact]
        public void GetToken()
        {
            var JwtPackage = new AzTokenClass();
            AppSettings settings = new AppSettings();
            settings.Key = key;
            settings.Issuer = "TestIssuer";
            var option = Options.Create(settings);
            AuthController controller = new AuthController(option); //Later move to test fixture, as same object can be reused.
            var token = controller.Token(JwtPackage) as OkObjectResult;
            var tokencontent = token.Value.ToString();
            Console.WriteLine("Result from controller is: {0}", tokencontent);
            Assert.NotNull(tokencontent);
            Assert.True(JwtValid(tokencontent));
        }
        [Fact]
        public void RefreshToken()
        {
            var JwtPackage = new AzTokenClass();
            AppSettings settings = new AppSettings();
            settings.Key = key;
            settings.Issuer = "TestIssuer";
            var option = Options.Create(settings);
            var controller = new AuthController(option); 
            var TestSepesToken = new SepesTokenClass();
            var token = controller.Token(JwtPackage) as OkObjectResult;
            TestSepesToken.idToken = token.Value.ToString(); //Tokencontent must be renamed in previous test and moved to class.
            var result = controller.RefreshToken(TestSepesToken) as OkObjectResult;
            Assert.True(JwtValid(result.Value.ToString()));
        }

        public bool JwtValid(string Token)//Check if token has valid formating.
        {
            bool result = true;
            try 
            {
                var attempt = new JwtSecurityTokenHandler().ReadToken(Token);
            }
            catch(Exception)
            {
                result = false;
            }
            return result;
        }

        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = "TestIssuer",
                ValidAudience = "Sample",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // The same key as the one that generate the token. TODO write dis!
            };
        }
    }
}