namespace HotelBooking.Domain;

/// <summary>
/// Thrown when an operation would break a business rule (e.g. checking in a cancelled booking).
/// The API translates it into a 4xx response.
/// </summary>
public class DomainException(string message) : Exception(message);
