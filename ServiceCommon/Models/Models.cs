﻿using System;

namespace ServiceCommon.Models
{
    using Orleans.Concurrency;
    
    [Immutable]
    [Serializable]
    public class Report
    {
        public Guid ParkId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public Location Location { get; set; }
        public Bounty Bounty { get; set; }
        public string Status { get; set; }
    }
    
    [Immutable]
    [Serializable]
    public class Bounty
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Gbp { get; set; }
        public string AchievmentImg { get; set; }
    }

    [Immutable]
    [Serializable]
    public class Location
    {
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}