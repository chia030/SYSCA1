namespace OrderApi.Infrastructure
{
    public interface ICustomerServiceGateway<T>
    {
        T Get(int id);

        //copied
        //IEnumerable<T> GetAll();
        //T Add(T entity);
        void Update(T entity, int id);
        //void Delete(int id);
    }
}