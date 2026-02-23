using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.ValueObjects;

namespace Mind_Manager.Domain.Validators;

public interface IUserValidator
{
    void ValidateEmail(string? email);
    void ValidatePhone(string? phone);
    void ValidatePassword(string? password);
}

public class UserValidator : IUserValidator
{
    public void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return;

        try
        {
            _ = new Email(email);
        }
        catch (ArgumentException)
        {
            throw new BusinessException("INVALID_EMAIL");
        }
    }

    public void ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return;

        try
        {
            _ = new Phone(phone);
        }
        catch (ArgumentException)
        {
            throw new BusinessException("INVALID_PHONE");
        }
    }

    public void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            throw new BusinessException("PASSWORD_INVALID");
    }
}
