using MauiStart.Models.Data.API.RequestProvider;
using MauiStart.Models.Data.UoW;
using MauiStart.Models.DTOs;
using MauiStart.Models.Services;

namespace MauiStart.Models.Domain.UseCases;

public class SendNewToDoItemUseCase : IUseCase<TodoItem, object> 
{
    private readonly IRequestProvider _requestProvider;
    
    public SendNewToDoItemUseCase(IRequestProvider requestProvider)
    {
        _requestProvider = requestProvider;
    }
    
    public async Task ExecuteAsync(TodoItem input, object callback = null)
    {
        var repositoriesUoW = ServiceHelper.GetService<IRepositoriesUoW>();
        try
        {
            await repositoriesUoW!.TodoItems.AddAsync(input);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        try
        {
            await _requestProvider.PostAsync(Constants.RestUrl, input);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}