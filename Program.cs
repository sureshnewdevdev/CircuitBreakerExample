
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace ServiceCallFailureSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            // Add HTTP client with Circuit Breaker policy
            builder.Services.AddHttpClient("ExternalApi", client =>
            {
                client.BaseAddress = new Uri("https://fgdfgjsonplaceholder.typicode.com"); // Example external API
            })
            .AddPolicyHandler(GetCircuitBreakerPolicy()); // Add Circuit Breaker

            builder.Services.AddControllers();


            // Add services to the container.


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        // Define the Circuit Breaker policy
        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles 5xx and network issues
                .Or<TimeoutRejectedException>() // Handles timeout exceptions
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3, // Number of exceptions before breaking
                    durationOfBreak: TimeSpan.FromSeconds(10), // Break duration
                    onBreak: (result, timespan) =>
                    {
                        Console.WriteLine("Circuit opened!");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit closed!");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("Circuit is half-open. Testing...");
                    });
        }
    }
}
