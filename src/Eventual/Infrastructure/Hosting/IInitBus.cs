namespace Eventual.Configuration
{
    using System.Threading.Tasks;

    public interface IInitBus
    {
        Task Start();
    }
}