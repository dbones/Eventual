namespace Eventual.Infrastructure.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(string content);
        string Serialize(object content);
    }
}