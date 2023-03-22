namespace OrderApi.Infrastructure
{
    public interface IServiceGateway<T>
    {
        T Get(int id);

        //copied
        //IEnumerable<T> GetAll();
        //T Add(T entity);
        //void Update(T entity);
        //void Delete(int id);
    }
}
