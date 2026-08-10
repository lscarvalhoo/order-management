using FluentAssertions;
using OrderManagement.Application.Commands.Login;

namespace OrderManagement.UnitTests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new LoginCommand("dev@martech.com", "Senha@123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string? email)
    {
        var command = new LoginCommand(email!, "ValidPassword123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid.com")]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        var command = new LoginCommand(email, "ValidPassword123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_EmptyPassword_ShouldHaveValidationError(string? password)
    {
        var command = new LoginCommand("dev@martech.com", password!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is required");
    }

    [Theory]
    [InlineData("12345")]     // 5 characters
    [InlineData("abc")]       // 3 characters
    [InlineData("1")]         // 1 character
    public void Validate_PasswordTooShort_ShouldHaveValidationError(string password)
    {
        var command = new LoginCommand("dev@martech.com", password);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Password" &&
            e.ErrorMessage == "Password must be at least 6 characters");
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("ValidPass")]
    [InlineData("Senha@123")]
    [InlineData("VeryLongPasswordThatIsStillValid")]
    public void Validate_ValidPasswordLength_ShouldNotHaveValidationError(string password)
    {
        var command = new LoginCommand("dev@martech.com", password);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_BothEmailAndPasswordInvalid_ShouldHaveMultipleErrors()
    {
        var command = new LoginCommand("invalidemail", "123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("user@example.com", "password123")]
    [InlineData("admin@company.org", "Admin@Pass")]
    [InlineData("test.user+tag@domain.co.uk", "Test123!@#")]
    public void Validate_VariousValidInputs_ShouldPass(string email, string password)
    {
        var command = new LoginCommand(email, password);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
