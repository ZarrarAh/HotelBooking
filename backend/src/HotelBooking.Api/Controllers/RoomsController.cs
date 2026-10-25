using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using HotelBooking.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(RoomService rooms) : ControllerBase
{
    /// <summary>Lists all rooms.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RoomResponse>>> GetAll() => await rooms.GetAllAsync();

    /// <summary>Finds rooms that are free for the whole stay and fit the number of guests.</summary>
    [HttpGet("available")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<AvailableRoomResponse>>> SearchAvailable([FromQuery] AvailabilityQuery query) =>
        await rooms.SearchAvailableAsync(query.CheckIn!.Value, query.CheckOut!.Value, query.Guests);

    /// <summary>Gets a single room.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponse>> GetById(int id)
    {
        var room = await rooms.GetByIdAsync(id);
        return room is null ? NotFound() : room;
    }

    /// <summary>Adds a room. Staff only.</summary>
    [Authorize(Roles = nameof(UserRole.Staff))]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Create(RoomRequest request)
    {
        var room = await rooms.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    /// <summary>Updates a room. Staff only.</summary>
    [Authorize(Roles = nameof(UserRole.Staff))]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Update(int id, RoomRequest request) =>
        await rooms.UpdateAsync(id, request);

    /// <summary>Deletes a room that has never been booked. Staff only.</summary>
    [Authorize(Roles = nameof(UserRole.Staff))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await rooms.DeleteAsync(id);
        return NoContent();
    }
}
