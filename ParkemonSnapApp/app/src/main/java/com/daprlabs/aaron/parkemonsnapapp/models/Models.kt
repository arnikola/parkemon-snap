package com.daprlabs.aaron.parkemonsnapapp.models

////        public Guid ParkId { get; set; }

//public string ImageUrl { get; set; }
//public string Description { get; set; }
//public Location Location { get; set; }
//public Bounty Bounty { get; set; }
//public string Status { get; set; }
/**
 * Created by Default on 30/07/2016.
 */

data class Park(var ParkId : Int , var ImageUrl : String, var Description : String, var Location : String, var Bounty : String, var Status : String)

//
//[Immutable]
//[Serializable]
//public class Bounty
//{
//    public string Title { get; set; }
//    public string Description { get; set; }
//    public int Gbp { get; set; }
//    public string AchievmentImg { get; set; }
//}

data class Bounty(var Title : String, var Description: String, var Gpb : Int, var AchievementImg : String)


//
//[Immutable]
//[Serializable]
//public class ParkData
//{
//    public Guid Id { get; set; }
//    public List<Location> GeoFence { get; set; }
//    public string Description { get; set; }
//}

data class ParkData(var Guid : Int, var GeoFence : String, var Description: String)



//
//[Immutable]
//[Serializable]
//public class Location
//{
//    public double Lat { get; set; }
//    public double Long { get; set; }
//}

data class Location(var Lat : Double, var Long : Double)

//[Immutable]
//[Serializable]
//public class FindResult
//{
//    public Location Location { get; set; }
//    public string Name { get; set; }
//    public Guid Id { get; set; }
//}

data class FindResult(var Location : Location, var Name : String, var Id : Int)