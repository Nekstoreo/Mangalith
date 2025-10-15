namespace Mangalith.Application.Interfaces.Services;

public interface IPasswordHasher
{
    string Hash(string value);
    bool Verify(string hash, string candidate);
}
