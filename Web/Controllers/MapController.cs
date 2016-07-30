using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Orleans;
using ServiceCommon.Actors;

namespace Web.Controllers
{
    using ServiceCommon.Models;

    [RoutePrefix("api/map")]
    public class MapController : ApiController
    {
        public SQLiteConnection GetConnection()
        {
            var assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var databaseFileName = Path.Combine(assemblyPath, "Data", "parks.db");
            var connection = new SQLiteConnection($"Data Source={databaseFileName};Version=3");
            connection.Open();
            return connection;
        }

#warning IMPLEMENT THIS
        [Route]
        [HttpGet]
        public List<FindResult> Get(double lat, double lon)
        {
            var from = new Location {Lat = lat, Long = lon};
            SQLiteConnection connection = null;
            try
            {
                const double radius = 0.5;
                connection = GetConnection();
                var idSearch =
                    new SQLiteCommand(
                        "select lat, lon, id from points where lat > @lat_lower and lat < @lat_upper and lon > @lon_lower and lon < @lon_upper",
                        connection);
                idSearch.Parameters.AddWithValue("lat_lower", lat - radius);
                idSearch.Parameters.AddWithValue("lat_upper", lat + radius);
                idSearch.Parameters.AddWithValue("lon_lower", lon - radius);
                idSearch.Parameters.AddWithValue("lon_upper", lon + radius);
                var ids = idSearch.ExecuteReader();

                var parkSearch =
                    new SQLiteCommand(
                        "select name, area from parks where rowid = @id limit 1",
                        connection);
                parkSearch.Prepare();
                var points = new Dictionary<int, List<Location>>();
                while (ids.Read())
                {
                    var id = ids.GetInt32(2);
                    List<Location> locs;
                    if (!points.TryGetValue(id, out locs))
                    {
                        locs = points[id] = new List<Location>();
                    }
                    locs.Add(new Location {Lat = ids.GetDouble(0), Long = ids.GetDouble(1)});
                }

                var nearest = points.SelectMany(kv =>
                    kv.Value.Select(
                        l =>
                            new LocDist {Id = kv.Key, Location = l, Distance = LocationUtility.GetKilometersTo(from, l)})
                        .OrderBy(_ => _.Distance)
                        .Take(1)
                    );

                var results = new List<FindResult>();
                foreach (var loc in nearest)
                {
                    parkSearch.Parameters.AddWithValue("id", loc.Id);
                    using (var inner = parkSearch.ExecuteReader())
                    {
                        inner.Read();
                        results.Add(new FindResult
                        {
                            Id = loc.Id,
                            Name = inner.GetString(0),
                            Location = loc.Location,
                            Distance = loc.Distance
                        });
                    }
                }

                results.Sort((a, b) => (int)(a.Distance - b.Distance));
                return results;
            }
            finally
            {
                connection?.Close();
                connection?.Dispose();
            }
        }
    }

    public class LocDist
    {
        public Location Location { get; set; }
        public double Distance { get; set; }
        public int Id { get; set; }
    }
}