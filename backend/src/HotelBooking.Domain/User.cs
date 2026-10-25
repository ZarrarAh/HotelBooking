namespace HotelBooking.Domain;

public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = "";
    public UserRole Role { get; private set; }

    // Required by EF Core.
    private User() { }

    public User(string email, string fullName, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        Role = role;
    }

    /// <param name="passwordHash">An already-hashed password; hashing is done by the API layer.</param>
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = passwordHash;
    }
}
