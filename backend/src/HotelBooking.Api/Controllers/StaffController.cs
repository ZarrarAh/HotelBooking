using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using HotelBooking.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

/// <summary>Front-desk views and actions.</summary>
[ApiController]
[Authorize(Roles = nameof(UserRole.Staff))]
[Route("api/staff")]
public class StaffController(ReservationService reservations) : ControllerBase
{
    /// <summary>Guests arriving on the date (defaults to today).</summary>
    [HttpGet("arrivals")]
    public async Task<ActionResult<List<ReservationResponse>>> Arrivals([FromQuery] DateOnly? date) =>
        await reservations.GetArrivalsAsync(date ?? reservations.Today);

    /// <summary>Guests departing on the date (defaults to today).</summary>
    [HttpGet("departures")]
    public async Task<ActionResult<List<ReservationResponse>>> Departures([FromQuery] DateOnly? date) =>
        await reservations.GetDeparturesAsync(date ?? reservations.Today);

    /// <summary>Checks a guest in. Only allowed during the stay.</summary>
    [HttpPost("reservations/{id:int}/check-in")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> CheckIn(int id) =>
        await reservations.CheckInAsync(id);

    /// <summary>Checks a guest out.</summary>
    [HttpPost("reservations/{id:int}/check-out")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> CheckOut(int id) =>
        await reservations.CheckOutAsync(id);
}
