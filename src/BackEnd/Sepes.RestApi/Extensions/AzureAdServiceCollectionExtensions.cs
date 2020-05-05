﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AzureAdServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder)
            => builder.AddAzureAdBearer(_ => { });

        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAzureOptions>();
            builder.AddJwtBearer();
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AzureAdOptions _azureOptions;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureOptions = azureOptions.Value;
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                options.Audience = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}/v2.0/";
                options.TokenValidationParameters.ValidIssuers = new List<string>
                {
                    "login.microsoftonline.com",
                    "sts.windows.net"
                };

                // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                // we inject our own multitenant validation logic (which even accepts both V1 and V2 tokens)
                options.TokenValidationParameters.ValidateIssuer = true;

                // If you want to use the V2 endpoint (that is authority = $"{_azureOptions.Instance}common/v2.0/") 
                // you'd also want to validate which tenants your Web API accept
                // in that case you'd have to implement a IssuerValidator and uncomment the following line.
                options.TokenValidationParameters.IssuerValidator = ValidateIssuer;

                //                options.TokenValidationParameters = new TokenValidationParameters
                //                {
                //                    RequireSignedTokens = false,
                ////                    ValidateIssuerSigningKey = true,
                ////                    ValidAudiences = new List<string>{ "client-id", "app-id-uri"},
                //                    ValidIssuers = new List<string>{
                //                        $"{_azureOptions.Instance}{_azureOptions.TenantId}/v2.0/",
                //                        $"https://sts.windows.net/{_azureOptions.TenantId}/"
                //                    }
                //                };
            }

            /// <summary>
            /// Validate the issuer. 
            /// </summary>
            /// <param name="issuer">Issuer to validate (will be tenanted)</param>
            /// <param name="securityToken">Received Security Token</param>
            /// <param name="validationParameters">Token Validation parameters</param>
            /// <remarks>The issuer is considered as valid if it has the same http scheme and authority as the
            /// authority from the configuration file, has a tenant Id, and optionally v2.0 (this web api
            /// accepts both V1 and V2 tokens)</remarks>
            /// <returns>The <c>issuer</c> if it's valid, or otherwise <c>null</c></returns>
            private string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
            {
                Uri uri = new Uri(issuer);
                Uri authorityUri = new Uri(_azureOptions.Instance);
                string[] parts = uri.AbsolutePath.Split('/');
                if (parts.Length >= 2)
                {
                    Guid tenantId;
                    if (uri.Scheme != authorityUri.Scheme || !validationParameters.ValidIssuers.Contains(uri.Authority))
                    {
                        throw new SecurityTokenInvalidIssuerException("Issuer has wrong authority");
                    }
                    if (!Guid.TryParse(parts[1], out tenantId))
                    {
                        throw new SecurityTokenInvalidIssuerException("Cannot find the tenant GUID for the issuer");
                    }
                    if (parts.Length > 2 && !(parts[2] == "v2.0" || parts[2] == ""))
                    {
                        throw new SecurityTokenInvalidIssuerException("Only accepted protocol versions are AAD v1.0 or V2.0");
                    }
                    return issuer;
                }
                else
                {
                    throw new SecurityTokenInvalidIssuerException("Unknown issuer");
                }
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
