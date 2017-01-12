using System;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public interface IModelState<T> 
        where T : class
    {
        Task<T> GetCurrentState(Guid id);
    }
}