namespace HockeyManager.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class PeopleRoot
    {
        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("people")]
        public People[] People { get; set; }
    }

    public partial class People
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("birthDate")]
        public DateTimeOffset BirthDate { get; set; }

        [JsonProperty("birthCity")]
        public string BirthCity { get; set; }

        [JsonProperty("birthStateProvince")]
        public string BirthStateProvince { get; set; }

        [JsonProperty("birthCountry")]
        public string BirthCountry { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("weight")]
        public long Weight { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("rookie")]
        public bool Rookie { get; set; }

        [JsonProperty("shootsCatches")]
        public string ShootsCatches { get; set; }

        [JsonProperty("rosterStatus")]
        public string RosterStatus { get; set; }

        [JsonProperty("primaryPosition")]
        public PrimaryPosition PrimaryPosition { get; set; }
    }

    public partial class PrimaryPosition
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("abbreviation")]
        public string Abbreviation { get; set; }
    }
}
