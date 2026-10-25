using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
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
}
