﻿using AutoMapper;
using Microsoft.Graph;
using Sepes.Azure.Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sepes.Azure.Dto;

namespace Sepes.Azure.Service
{
    public class AzureUserService : IAzureUserService
    {
        readonly IMapper _mapper;
        readonly IGraphServiceProvider _graphServiceProvider;      

        public AzureUserService(IMapper mapper, IGraphServiceProvider graphServiceProvider)
        {
            _mapper = mapper;
            _graphServiceProvider = graphServiceProvider;
        }
        public async Task<List<Microsoft.Graph.User>> SearchUsersAsync(string search, int limit, CancellationToken cancellationToken = default)
        {
            List<Microsoft.Graph.User> listUsers = new List<User>();

            if (string.IsNullOrWhiteSpace(search))
            {
                return listUsers;
            }

            // Initialize the GraphServiceClient.            
            GraphServiceClient graphClient = _graphServiceProvider.GetGraphServiceClient(new[] { "User.Read.All" });

            var graphRequest = graphClient.Users.Request().Top(limit).Filter($"startswith(displayName,'{search}') or startswith(givenName,'{search}') or startswith(surname,'{search}') or startswith(mail,'{search}') or startswith(userPrincipalName,'{search}')");

            
            while (true)
            {
                if(graphRequest == null || listUsers.Count > limit)
                {
                    break;
                }
                var response = await graphRequest.GetAsync(cancellationToken: cancellationToken);

                 listUsers.AddRange(response.CurrentPage);                    
                
                graphRequest = response.NextPageRequest;
            } 

            return listUsers;

        }
        public async Task<AzureUserDto> GetUserAsync(string id)
        {
            // Initialize the GraphServiceClient. 
            GraphServiceClient graphClient = _graphServiceProvider.GetGraphServiceClient(new[] { "User.Read" });

            var response = await graphClient.Users.Request().Filter($"Id eq '{id}'").GetAsync();

            var firstResponseItem = response.FirstOrDefault();

            return _mapper.Map<AzureUserDto>(firstResponseItem);
        }      
    }
}