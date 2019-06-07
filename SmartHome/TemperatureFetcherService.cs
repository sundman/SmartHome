using System;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using Logger;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;

namespace SmartHome
{
    public class TemperatureFetcherService : IHostedService
    {
        private readonly SplunkLogger _logger;
        private Timer _timer;
        private string apiToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCIsImF1ZCI6IkxvZ2dlciIsImV4cCI6MTU5MDMyNzQ5Nn0.eyJyZW5ldyI6dHJ1ZSwidHRsIjozMTUzNjAwMH0.Z_BjlBdioX_4OiLXnBHvCjp1DLj4I94eKZ7Mml8wk7Y";

        private string baseUri = "http://home.sundman.at:8888/api";

        public TemperatureFetcherService(SplunkLogger logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CallHome, null, 0, 1 * 60 * 1000);
            return Task.CompletedTask;
        }


        void CallHome(object state)
        {

            try
            {
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);


                var sensorList = client.GetStringAsync($"{baseUri}/sensors/list");
                var listResult = JsonConvert.DeserializeObject<SensorList>(sensorList.Result);


                foreach (var sensor in listResult.sensor)
                {
                    try
                    {
                        var streamTask = client.GetStringAsync($"{baseUri}/sensor/info/?id={sensor.id}");
                        var result = JsonConvert.DeserializeObject<SensorReading>(streamTask.Result);

                        foreach (var r in result.data)
                        {

                            _logger.Info(this, new SensorEvent()
                            {
                                DeviceName = result.name,
                                Name = r.name,
                                Value = r.value
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(this, new ExceptionEvent(ex));

                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error(this, new ExceptionEvent(ex));

            }


        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

    }


    public class Sensor
    {
        public int id { get; set; }
        public string model { get; set; }
        public string name { get; set; }
        public bool novalues { get; set; }
        public string protocol { get; set; }
        public int sensorId { get; set; }
    }

    public class SensorList
    {
        public List<Sensor> sensor { get; set; }
    }


    public class ExceptionEvent : ILoggable
    {
        public ExceptionEvent(Exception ex)
        {
            Message = ex.Message;
            StackTrace = ex.StackTrace;

            if (ex.InnerException != null)
            {
                InnerException = new ExceptionEvent(ex.InnerException);
            }
        }

        public string Message { get; }
        public string StackTrace { get; }
        public ExceptionEvent InnerException { get; }
    }

    public class SensorEvent : ILoggable
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string DeviceName { get; set; }

    }

    public class SensorData
    {
        public string name { get; set; }
        public int scale { get; set; }
        public double value { get; set; }
    }

    public class SensorReading
    {
        public List<SensorData> data { get; set; }
        public int id { get; set; }
        public string model { get; set; }
        public string name { get; set; }
        public string protocol { get; set; }
        public int sensorId { get; set; }
    }
}