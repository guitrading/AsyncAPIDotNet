using FlightPlanApi.Data;
using FlightPlanApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace FlightPlanApi.Controllers
{
    [Route("api/v1/flightplan")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        // private field to hold the database adapter
        private IDatabaseAdapter _database;

        public FlightPlanController(IDatabaseAdapter database)
        {
            _database = database;
        }

        // Method signatures

        [HttpGet]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "No flight plans in the system")]
        public async Task<IActionResult> FlightPlanList()
        {
            var flightPlanList = await _database.GetAllFlightPlans();
            if (flightPlanList.Count == 0)
            {
                return StatusCode(StatusCodes.Status204NoContent); // or NoContent()
            }
            return Ok(flightPlanList);
        }


        [HttpGet]
        [Authorize]
        [Route("{flightPlanId}")] // Curly braces indicate a variable matching the parameter
        public async Task<IActionResult> GetFlightPlanById(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);
            if (flightPlan.FlightPlanId != flightPlanId)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return Ok(flightPlan);
        }

        /// <summary>
        /// Files new flight plan
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        ///     POST /api/v1/flightplan/file
        ///     {
        ///         "aircraft_identification": "N67SVS",
        ///         "aircraft_type": "Cessna 172",
        ///         "airspeed": 100,
        ///         "altitute": 10000,
        ///         "flight_type": "VFR",
        ///         "fuel_hours": 1,
        ///         "fuel_minutes": 30,
        ///         "departure_time": "2020-05-01T12:00:00Z",
        ///         "estimated_arrival_time": "2020-05-01T13:30:00Z",
        ///         "departuring_airport": "KTTN",
        ///         "arrival_airport": "KNZY",
        ///         "route": "KTTN..RBV..DIXIE..KNZY",
        ///         "remarks": "user remarks",
        ///         "number_onboard": 1
        ///     }
        /// </remarks>
        /// <param name="flightPlan">Flight plan to be filed.</param>
        /// <response code="400">If the flight plan is invalid.</response>
        /// <response code="500">If there was an error filing the flight plan.</response>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("file")] // string literal not a variable

        public async Task<IActionResult> FileFlightPlan(FlightPlan flightPlan)
        {
            var transactionResult = await _database.FileFlightPlan(flightPlan);
            switch (transactionResult)
            {
                case TransactionResult.Success:
                    return StatusCode(StatusCodes.Status201Created);
                case TransactionResult.BadRequest:
                    return StatusCode(StatusCodes.Status400BadRequest);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateFlightPlan(FlightPlan flightPlan)
        {
            var transactionResult = await _database.UpdateFlightPlan(flightPlan.FlightPlanId, flightPlan);
            switch (transactionResult)
            {
                case TransactionResult.Success:
                    return StatusCode(StatusCodes.Status200OK);
                case TransactionResult.BadRequest:
                    return StatusCode(StatusCodes.Status400BadRequest);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("{flightPlanId}")]
        public async Task<IActionResult> DeleteFlightPlan(string flightPlanId)
        {
            var result = await _database.DeleteFlightPlan(flightPlanId);
            if (result)
            {
                return StatusCode(StatusCodes.Status200OK);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet]
        [Authorize]
        [Route("airport/departure/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanDepartureAirport(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);
            if (flightPlan.FlightPlanId == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return Ok(flightPlan.DeparturingAirport);
        }

        [HttpGet]
        [Authorize]
        [Route("route/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanRoute(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);
            if (flightPlan.FlightPlanId == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return Ok(flightPlan.Route);
        
        }

        [HttpGet]
        [Authorize]
        [Route("time/enroute/{flightPlanId}")]
        public async Task<IActionResult> GetFlightPlanTimeEnroute(string flightPlanId)
        {
            var flightPlan = await _database.GetFlightPlanById(flightPlanId);
            if (flightPlan.FlightPlanId == null)
        {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            var estimatedTimeEnRoute = flightPlan.ArrivalTime - flightPlan.DepartureTime;
            return Ok(estimatedTimeEnRoute);
        }
    }
}
