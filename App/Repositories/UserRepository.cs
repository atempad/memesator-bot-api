using App.Models.DB;
using App.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace App.Repositories;

public class UserRepository(
    CosmosClient cosmosClient,
    IOptions<DbSettings> dbSettings) 
    : CosmosContainerRepository<ServiceUser>(cosmosClient, dbSettings.Value.DatabaseId, Constants.DB.Containers.Users), 
        IUserRepository;