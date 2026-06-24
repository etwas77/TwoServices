using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Config
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = string.Empty;
        public ushort Port { get; set; } = 0;
        public string ManagementUrl { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string OrderQueue { get; set; } = string.Empty;

        public string FailedOrdersQueue { get; set; } = string.Empty;

        // retry stuff
        public string RetryQueue { get; set; } = string.Empty;
        public int RetryDelayMilliseconds { get; set; } = 5000;
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
