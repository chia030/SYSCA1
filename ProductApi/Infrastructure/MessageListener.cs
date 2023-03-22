using System;
using System.Threading;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkCompleted",
                    HandleOrderCompleted, x => x.WithTopic("completed"));

                //idk if the "productApiOrderCancelled" is correct
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiOrderCancelled",
                    HandleOrderCancelled, x => x.WithTopic("cancelled"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiOrderShipped",
                    HandleOrderShipped, x => x.WithTopic("shipped"));

                bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiOrderPaid",
                    HandleOrderPaid, x => x.WithTopic("paid"));


                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }

        }

        private void HandleOrderCompleted(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        //this one is ready to be used when listening to cancelled
        private void HandleOrderCancelled(OrderStatusChangedMessage message) 
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity; //item no longer reserved
                    product.ItemsInStock += orderLine.Quantity; //item goes back in stock
                    productRepos.Edit(product);
                }
            }
        }

        //this one is ready to be used when listening to shipped
        private void HandleOrderShipped(OrderStatusChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity; //item no longer reserved or in stock
                    productRepos.Edit(product);
                }
            }
        }

        //this one is ready to be used when listening to shipped
        private void HandleOrderPaid(OrderStatusChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity; //item no longer reserved or in stock //shipped and paid do the same thing in Products?
                    productRepos.Edit(product);
                }
            }
        }

    }
}
