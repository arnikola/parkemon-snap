using System;
using System.Collections.Generic;

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
    public class ParkData
    {
        public Guid Id { get; set; }
        public List<Location> GeoFence { get; set; }
        public string Description { get; set; }
    }

    [Immutable]
    [Serializable]
    public class Location
    {
        public double Lat { get; set; }
        public double Long { get; set; }
    }

    [Immutable]
    [Serializable]
    public class FindResult
    {
        public Location Location { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
    }

    public static class LocationUtility
    {
        public static double GetKilometersTo(Location one, Location other)
        {
            const double EarthRadiusInKilometers = 6371.0;
            return EarthRadiusInKilometers*GetDistanceRatioTo(one, other);
        }

        public static double GetDistanceRatioTo(Location one, Location other)
        {
            const double DegreesToRadians = Math.PI/180;
            var latRadiansA = one.Lat*DegreesToRadians;
            var lonRadiansA = one.Long*DegreesToRadians;
            var latRadiansB = other.Lat*DegreesToRadians;
            var lonRadiansB = other.Long*DegreesToRadians;
            var lonDelta = lonRadiansB - lonRadiansA;
            var latDelta = latRadiansB - latRadiansA;
            var d = Math.Pow(Math.Sin(latDelta/2.0), 2.0) +
                    ((Math.Cos(latRadiansA)*Math.Cos(latRadiansB))*Math.Pow(Math.Sin(lonDelta/2.0), 2.0));
            return 2.0*Math.Atan2(Math.Sqrt(d), Math.Sqrt(1.0 - d));
        }
    }
}