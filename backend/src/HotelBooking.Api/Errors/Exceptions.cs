namespace HotelBooking.Api.Errors;

/// <summary>The requested resource doesn't exist (or isn't visible to the caller). Maps to 404.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>The request clashes with the current state, e.g. the room was just booked. Maps to 409.</summary>
public class ConflictException(string message) : Exception(message);
