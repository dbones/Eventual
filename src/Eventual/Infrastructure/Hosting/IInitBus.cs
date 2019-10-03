namespace Eventual.Infrastructure.Hosting
{
    using System.Threading.Tasks;

    public interface IInitBus
    {
        Task Start();
    }
}