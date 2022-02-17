using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DotnetToLambda.Core.Infrastructure
{
    public class DatabaseConnection
    {
        private string _connectionString;

        public DatabaseConnection() { }

        public DatabaseConnection(string fullConnectionString)
        {
            this._connectionString = fullConnectionString;
        }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("engine")]
        public string Engine { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("dbInstanceIdentifier	")]
        public string DbInstanceIdentifier { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this._connectionString))
            {
                return this._connectionString;
            }

            return
                $"server={this.Host};port={this.Port};database=BookingDB;user={this.Username};password={this.Password};Persist Security Info=False;Connect Timeout=300";
        }
    }
}
