using HotelBooking.Api.Auth;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using HotelBooking.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public class ReservationsController(ReservationService reservations) : ControllerBase
{
    private bool IsStaff => User.IsInRole(nameof(UserRole.Staff));

    /// <summary>Books a room for the signed-in guest.</summary>
    [Authorize(Roles = nameof(UserRole.Guest))]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request)
    {
        var reservation = await reservations.CreateAsync(User.GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    /// <summary>Lists the signed-in guest's reservations, newest stay first.</summary>
    [Authorize(Roles = nameof(UserRole.Guest))]
    [HttpGet("mine")]
    public async Task<ActionResult<List<ReservationResponse>>> GetMine() =>
        await reservations.GetForGuestAsync(User.GetUserId());

    /// <summary>Gets a reservation. Guests can only see their own.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetById(int id) =>
        await reservations.GetAsync(id, User.GetUserId(), IsStaff);

    /// <summary>Cancels a confirmed reservation. Guests can only cancel their own.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> Cancel(int id) =>
        await reservations.CancelAsync(id, User.GetUserId(), IsStaff);
}
