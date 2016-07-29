using System;
using System.Collections.Generic;

namespace ServiceCommon.Models
{
    using Orleans.Concurrency;
    
    [Serializable]
    internal enum HackathonStatus
    {
        Won,
        NotWonYet,
        KillingIt
    }
    
    [Immutable]
    [Serializable]
    public class Update
    {
        public ParkDetails UpdateString { get; set; }
    }
    
    [Immutable]
    [Serializable]
    public class ParkDetails
    {
        public Guid Id { get; set; }
        public string Details { get; set; }
    }
    
}
