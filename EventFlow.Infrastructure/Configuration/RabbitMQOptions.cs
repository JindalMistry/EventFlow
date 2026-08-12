using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Worker.Configurations
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
