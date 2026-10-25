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

    /// <summary>Gets a single room.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponse>> GetById(int id)
    {
        var room = await rooms.GetByIdAsync(id);
        return room is null ? NotFound() : room;
    }
}
