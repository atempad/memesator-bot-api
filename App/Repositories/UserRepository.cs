using App.Models.DB;
using Microsoft.Azure.Cosmos;

namespace App.Repositories;

public class UserRepository(CosmosClient cosmosClient) 
    : CosmosContainerRepository<ServiceUser>(cosmosClient, Constants.DB.Id, Constants.DB.Containers.Users), 
        IUserRepository;