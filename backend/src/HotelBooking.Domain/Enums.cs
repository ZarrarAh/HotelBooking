namespace HotelBooking.Domain;

public enum RoomType
{
    Single,
    Double,
    Suite
}

public enum UserRole
{
    Guest,
    Staff
}

public enum ReservationStatus
{
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled
}
