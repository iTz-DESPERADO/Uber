using Uber.Domain.Common;
using Xunit;

namespace Uber.Domain.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_preserves_error()
    {
        var error = new Error("Ride.NotFound", "Ride not found");
        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Same(error, result.Error);
    }

    [Fact]
    public void Success_preserves_value_and_can_be_used_as_an_outcome()
    {
        var value = new object();
        var result = Result<object>.Success(value);
        Result outcome = result;

        Assert.Same(value, result.Value);
        Assert.True(outcome.IsSuccess);
        Assert.False(outcome.IsFailure);
        Assert.Null(outcome.Error);
    }

    [Fact]
    public void Default_value_is_a_valid_success()
    {
        var result = Result<int>.Success(0);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Nullable_success_preserves_absence_without_inventing_an_error()
    {
        var result = Result<string?>.Success(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Failed_value_cannot_be_read_as_a_default_value()
    {
        var error = new Error("Ride.NotFound", "Ride not found");
        var result = Result<int>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Same(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_requires_an_error()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
        Assert.Throws<ArgumentNullException>(() => Result<int>.Failure(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Error_requires_a_code(string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Error(code!, "Ride not found"));
    }

    [Fact]
    public void Error_rejects_null_message()
    {
        Assert.Throws<ArgumentNullException>(() => new Error("Ride.NotFound", null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Error_preserves_blank_legacy_messages(string message)
    {
        var result = Result.Failure(new Error("Ride.UpdateFailed", message));
        Assert.True(result.IsFailure);
        Assert.Equal(message, result.Error!.Message);
    }

    [Fact]
    public void Errors_use_value_equality_and_preserve_text()
    {
        var error = new Error("Ride.NotFound", "Ride not found");

        Assert.Equal("Ride.NotFound", error.Code);
        Assert.Equal("Ride not found", error.Message);
        Assert.Equal(error, new Error("Ride.NotFound", "Ride not found"));
        Assert.NotEqual(error, new Error("Ride.InvalidStatus", "Ride not found"));
    }
}
