using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using FlightPlanApi.Models;
using FlightPlanApi.Data;

namespace FlightPlanApi.Data

{
    public class MongoDbDatabase: IDatabaseAdapter
    {
        public async Task<List<FlightPlan>> GetAllFlightPlans()
        {
            var collection = GetCollection("flightplans", "flightplans");
            var documents = collection.Find(_ => true).ToListAsync();

            var flightPlansList = new List<FlightPlan>();

            if (documents == null) return flightPlansList;

            foreach (var document in await documents)
            {
                flightPlansList.Add(ConvertBsonToFlightPlan(document));
            }
            return flightPlansList;
        }

        public async Task<FlightPlan> GetFlightPlanById(string flightPlanId)
        {
            var collection = GetCollection("flightplans", "flightplans");
            var flightPlanCursor = await collection.FindAsync(
                Builders<BsonDocument>.Filter.Eq("flight_plan_id", flightPlanId));
            var document = flightPlanCursor.FirstOrDefault();
            var flightPlan = ConvertBsonToFlightPlan(document);

            if (document == null)
            {
                return new FlightPlan();
            }

            return flightPlan;
        }

        public async Task<TransactionResult> FileFlightPlan(FlightPlan flightPlan)
        {
            var collection = GetCollection("flightplans", "flightplans");
            var document = new BsonDocument
            {
                {"flight_plan_id", Guid.NewGuid().ToString("N")},
                {"aircraft_identification", flightPlan.AircraftIdentification},
                {"aircraft_type", flightPlan.AircraftType},
                {"airspeed", flightPlan.Airspeed},
                {"altitude", flightPlan.Altitude},
                {"flight_type", flightPlan.FlightType},
                {"fuel_hours", flightPlan.FuelHours},
                {"fuel_minutes", flightPlan.FuelMinutes},
                {"departure_time", flightPlan.DepartureTime},
                {"estimated_arrival_time", flightPlan.ArrivalTime},
                {"departuring_airport", flightPlan.DeparturingAirport},
                {"arrival_airport", flightPlan.ArrivalAirport},
                {"route", flightPlan.Route},
                {"remarks", flightPlan.Remarks},
                {"number_onboard", flightPlan.NumberOnboard}
            };

            try
            {
                await collection.InsertOneAsync(document);
                if (document["_id"].IsObjectId)
                {
                    return TransactionResult.Success;
                }
                return TransactionResult.BadRequest;
            }
            catch
            {
                return TransactionResult.ServerError;
            }
            
        }

        public async Task<TransactionResult> UpdateFlightPlan(string flightPlanId, FlightPlan flightPlan)
        {
            var collection = GetCollection("flightplans", "flightplans");
            var filter = Builders<BsonDocument>.Filter.Eq("flight_plan_id", flightPlanId);
            var update = Builders<BsonDocument>.Update
                .Set("aircraft_identification", flightPlan.AircraftIdentification)
                .Set("aircraft_type", flightPlan.AircraftType)
                .Set("airspeed", flightPlan.Airspeed)
                .Set("altitude", flightPlan.Altitude)
                .Set("flight_type", flightPlan.FlightType)
                .Set("fuel_hours", flightPlan.FuelHours)
                .Set("fuel_minutes", flightPlan.FuelMinutes)
                .Set("departure_time", flightPlan.DepartureTime)
                .Set("estimated_arrival_time", flightPlan.ArrivalTime)
                .Set("departuring_airport", flightPlan.DeparturingAirport)
                .Set("arrival_airport", flightPlan.ArrivalAirport)
                .Set("route", flightPlan.Route)
                .Set("remarks", flightPlan.Remarks)
                .Set("numberOnBoard", flightPlan.NumberOnboard);
            var result = await collection.UpdateOneAsync(filter, update);
            
            if(result.MatchedCount == 0)
            {
                return TransactionResult.NotFound;
            }
            if(result.ModifiedCount > 0)
            {
                return TransactionResult.Success;
            }
            return TransactionResult.ServerError;
        }

        public async Task<bool> DeleteFlightPlan(string flightPlanId)
        {
            var collection = GetCollection("flightplans", "flightplans");
            var result = await collection.DeleteOneAsync(
                               Builders<BsonDocument>.Filter.Eq("flight_plan_id", flightPlanId));
            return result.DeletedCount > 0;
        }


        // Create a method to get a collection as it will be used in multiple places
        private IMongoCollection<BsonDocument> GetCollection(string databaseName, string collectionName)
        {
            // To get a collection we need a connection to the database
            // This is done by creating a MongoClient and then getting the database from that client
            var client = new MongoClient();
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection;
        }

        // Method to convert Bsondocument to FlightPlan
        private FlightPlan ConvertBsonToFlightPlan(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }
            return new FlightPlan
            {
                FlightPlanId = document["flight_plan_id"].AsString,
                AircraftIdentification = document["aircraft_identification"].AsString,
                AircraftType = document["aircraft_type"].AsString,
                Airspeed = document["airspeed"].AsInt32,
                Altitude = document["altitude"].AsInt32,
                FlightType = document["flight_type"].AsString,
                FuelHours = document["fuel_hours"].AsInt32,
                FuelMinutes = document["fuel_minutes"].AsInt32,
                DepartureTime = document["departure_time"].ToUniversalTime(),
                ArrivalTime = document["estimated_arrival_time"].ToUniversalTime(),
                DeparturingAirport = document["departuring_airport"].AsString,
                ArrivalAirport = document["arrival_airport"].AsString,
                Route = document["route"].AsString,
                Remarks = document["remarks"].AsString,
                NumberOnboard = document["number_onboard"].AsInt32
            };
        }         
    }
}
