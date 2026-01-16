using MauiStart.Models.Data;
using MauiStart.Models.Data.API.RequestProvider;
using MauiStart.Models.Data.UoW;
using MauiStart.Models.DTOs;
using MauiStart.Models.Services;
using Microsoft.EntityFrameworkCore;

namespace MauiStart.Models.Domain.UseCases;

public class RetrieveToDoItemsUseCase : IUseCase<IEnumerable<TodoItem>>
{
    private readonly IRequestProvider _requestProvider;
    private readonly CachePolicy _cachePolicy;
    
    public RetrieveToDoItemsUseCase(IRequestProvider requestProvider, CachePolicy cachePolicy)
    {
        _requestProvider = requestProvider;
        _cachePolicy = cachePolicy;
    }
    
    public async Task<IEnumerable<TodoItem>> ExecuteAsync()
    {
        var repositoriesUoW = ServiceHelper.GetService<IRepositoriesUoW>();
        
        var localData = repositoriesUoW.TodoItems.Query;
        if (localData.Any() && !_cachePolicy.IsExpired())
        {
            return localData;
        }

        List<TodoItem> remoteData = new List<TodoItem>();
        
        try
        {
            remoteData = await _requestProvider.GetAsync<List<TodoItem>>(Constants.RestUrl);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return localData;
        }
        
        foreach (var item in remoteData)
        {
            try
            {
                await repositoriesUoW.TodoItems.AddAsync(item);
            }
            catch (DbUpdateException e)
            {
                // if the item already exist
                repositoriesUoW.TodoItems.Update(item);
            }
        }
        
        _cachePolicy.LastUpdated = DateTime.UtcNow;
        return remoteData;
    }
} 