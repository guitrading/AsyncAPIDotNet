using System.Collections.Generic;
using System.Threading.Tasks;
using FlightPlanApi.Models;

namespace FlightPlanApi.Data
{
    // Interface to be implemented by the database adapter to make async calls to the database
    // Some tasks return a boolean value to indicate success or failure with allows this to be
    // used with any database
    public interface IDatabaseAdapter
    {
        Task<FlightPlan> GetFlightPlanById(string flightPlanId);
        Task<List<FlightPlan>> GetAllFlightPlans();
        Task<TransactionResult> FileFlightPlan(FlightPlan flightPlan);
        Task<TransactionResult> UpdateFlightPlan(string flightPlanId, FlightPlan flightPlan);
        Task<bool> DeleteFlightPlan(string flightPlanId);
    }
}
