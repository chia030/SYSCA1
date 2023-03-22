using System;
using RestSharp;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class CustomerServiceGateway : IServiceGateway<CustomerDto>
    {
        string customerServiceBaseUrl;

        public CustomerServiceGateway(string baseUrl)
        {
            customerServiceBaseUrl = baseUrl;
        }

        public CustomerDto Get(int id)
        {
            RestClient c = new RestClient(customerServiceBaseUrl);

            var request = new RestRequest(id.ToString());
            var response = c.GetAsync<CustomerDto>(request);
            response.Wait();
            return response.Result;
        }

        //experimental put method
        public void Update(CustomerDto customer, int id)
        {
            RestClient client= new RestClient(customerServiceBaseUrl);

            var request = new RestRequest("customers/" + id, Method.Put);
            request.AddJsonBody(customer);
            client.Execute(request);
        }
    }
}
