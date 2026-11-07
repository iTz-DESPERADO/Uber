using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Uber.BLL.ModelVM.Ride;
using Uber.BLL.Services.Abstraction;
using Uber.BLL.Services.Impelementation;
using Uber.DAL.Entities;
using Uber.Domain.Enums;
using Uber.DAL.Repo.Abstraction;
using Uber.Ui.Controllers;
using Xunit;

namespace Uber.Behavior.Tests;

public sealed class RideTransitionTests
{
    private const int RideId = 42;

    public static IEnumerable<object[]> Transitions()
    {
        yield return ["DriverArrived", RideStatus.Accepted, RideStatus.DriverWaiting,
            "Cannot mark driver as arrived", "DriverArrived", "Driver arrived at pickup location",
            "Failed to mark driver as arrived"];
        yield return ["StartRide", RideStatus.DriverWaiting, RideStatus.InProgress,
            "Cannot start ride", "RideStarted", "Ride started successfully", "Failed to start ride"];
        yield return ["CompleteRide", RideStatus.InProgress, RideStatus.Completed,
            "Cannot complete ride", "RideCompleted", "Ride completed successfully", "Failed to complete ride"];
    }

    public static IEnumerable<object[]> StatusCases() =>
        from transition in Transitions()
        from status in Enum.GetValues<RideStatus>().Append((RideStatus)99)
        select transition.Take(6).Append((object)status).ToArray();

    public static IEnumerable<object[]> LookupCases() => Transitions().Select(row => new[] { row[0], row[1] });
    public static IEnumerable<object[]> UpdateCases() => Transitions().Select(row => new[] { row[0], row[1], row[2], row[6] });
    public static IEnumerable<object[]> ExceptionCases() => Transitions().Select(row => row.Take(3).ToArray());
    public static IEnumerable<object[]> NotificationCases() => Transitions().Select(row => new[] { row[0], row[1], row[2], row[4] });
    public static IEnumerable<object[]> EndpointCases() => Transitions().Select(row => new[] { row[0] });

    [Theory]
    [MemberData(nameof(StatusCases))]
    public async Task Status_matrix_preserves_responses_writes_and_notifications(
        string action, RideStatus required, RideStatus target, string errorPrefix,
        string eventName, string message, RideStatus current)
    {
        var fixture = new Fixture();
        fixture.Repository.Setup(repo => repo.GetByID(RideId)).Returns((null!, NewRide(current)));
        var allowed = current == required;
        if (allowed)
        {
            fixture.Repository.Setup(repo => repo.UpdateStatus(RideId, target))
                .Callback(() => fixture.Events.Add("update"))
                .Returns((true, (string?)null));
            fixture.AllowNotification(eventName);
        }

        var response = await Invoke(fixture.Controller, action);

        if (allowed)
        {
            var ok = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, ok.StatusCode);
            var payload = JsonSerializer.SerializeToElement(ok.Value);
            Assert.Equal(2, payload.EnumerateObject().Count());
            Assert.True(payload.GetProperty("success").GetBoolean());
            Assert.Equal(message, payload.GetProperty("message").GetString());
            Assert.Equal(new[] { "update", "notify" }, fixture.Events);
            fixture.Repository.Verify(repo => repo.UpdateStatus(RideId, target), Times.Once);
            fixture.VerifyNotification(eventName);
        }
        else
        {
            AssertBadRequest(response, $"{errorPrefix}. Ride is currently in {current} status. Expected: {required}");
            fixture.VerifyNoSideEffects();
        }
        fixture.Repository.Verify(repo => repo.GetByID(RideId), Times.Once);
        fixture.Repository.VerifyNoOtherCalls();
    }

    [Theory]
    [MemberData(nameof(LookupCases))]
    public async Task Missing_ride_or_lookup_error_returns_not_found_without_writing(string action, RideStatus required)
    {
        foreach (var lookup in new (string?, Ride?)[]
                 { (null, null), ("lookup failed", null), ("lookup failed", NewRide(required)) })
        {
            var fixture = new Fixture();
            fixture.Repository.Setup(repo => repo.GetByID(RideId)).Returns((lookup.Item1!, lookup.Item2));

            AssertBadRequest(await Invoke(fixture.Controller, action), "Ride not found");

            fixture.VerifyNoSideEffects();
            fixture.Repository.Verify(repo => repo.GetByID(RideId), Times.Once);
            fixture.Repository.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [MemberData(nameof(UpdateCases))]
    public async Task Failed_update_preserves_error_or_controller_fallback(string action, RideStatus required, RideStatus target, string fallback)
    {
        foreach (var error in new string?[] { "write failed", null, "" })
        {
            var fixture = new Fixture();
            fixture.Repository.Setup(repo => repo.GetByID(RideId)).Returns((null!, NewRide(required)));
            fixture.Repository.Setup(repo => repo.UpdateStatus(RideId, target)).Returns((false, error));

            AssertBadRequest(await Invoke(fixture.Controller, action), error ?? fallback);

            fixture.Repository.Verify(repo => repo.GetByID(RideId), Times.Once);
            fixture.Repository.Verify(repo => repo.UpdateStatus(RideId, target), Times.Once);
            fixture.Repository.VerifyNoOtherCalls();
            fixture.Hub.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [MemberData(nameof(ExceptionCases))]
    public async Task Repository_exceptions_become_service_error_messages(string action, RideStatus required, RideStatus target)
    {
        foreach (var throwDuringUpdate in new[] { false, true })
        {
            var fixture = new Fixture();
            if (throwDuringUpdate)
            {
                fixture.Repository.Setup(repo => repo.GetByID(RideId)).Returns((null!, NewRide(required)));
                fixture.Repository.Setup(repo => repo.UpdateStatus(RideId, target))
                    .Throws(new InvalidOperationException("database unavailable"));
            }
            else
            {
                fixture.Repository.Setup(repo => repo.GetByID(RideId))
                    .Throws(new InvalidOperationException("database unavailable"));
            }

            AssertBadRequest(await Invoke(fixture.Controller, action), "database unavailable");

            fixture.Repository.Verify(repo => repo.GetByID(RideId), Times.Once);
            fixture.Repository.Verify(repo => repo.UpdateStatus(RideId, target),
                throwDuringUpdate ? Times.Once() : Times.Never());
            fixture.Repository.VerifyNoOtherCalls();
            fixture.Hub.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [MemberData(nameof(NotificationCases))]
    public async Task Notification_failure_returns_bad_request_after_status_update_without_retry(string action, RideStatus required, RideStatus target, string eventName)
    {
        var fixture = new Fixture();
        fixture.Repository.Setup(repo => repo.GetByID(RideId)).Returns((null!, NewRide(required)));
        fixture.Repository.Setup(repo => repo.UpdateStatus(RideId, target))
            .Callback(() => fixture.Events.Add("update")).Returns((true, (string?)null));
        fixture.AllowNotification(eventName, new InvalidOperationException("notification failed"));

        AssertBadRequest(await Invoke(fixture.Controller, action), "An error occurred: notification failed");

        Assert.Equal(new[] { "update", "notify" }, fixture.Events);
        fixture.Repository.Verify(repo => repo.GetByID(RideId), Times.Once);
        fixture.Repository.Verify(repo => repo.UpdateStatus(RideId, target), Times.Once);
        fixture.Repository.VerifyNoOtherCalls();
        fixture.VerifyNotification(eventName);
    }

    [Theory]
    [MemberData(nameof(EndpointCases))]
    public void Endpoints_keep_driver_authorization_post_and_body_binding(string action)
    {
        var method = typeof(RideController).GetMethod(action)!;
        var authorization = Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Driver", authorization.Roles);
        Assert.Single(method.GetCustomAttributes(typeof(HttpPostAttribute), true));
        Assert.Single(method.GetParameters()[0].GetCustomAttributes(typeof(FromBodyAttribute), true));
    }

    private static Ride NewRide(RideStatus status) => new() { Status = status, UserId = "rider", DriverId = "driver" };

    private static Task<IActionResult> Invoke(RideController controller, string action)
    {
        var request = new AcceptRejectRequest { id = RideId, rideGroup = "untrusted-client-group" };
        return action switch
        {
            "DriverArrived" => controller.DriverArrived(request),
            "StartRide" => controller.StartRide(request),
            "CompleteRide" => controller.CompleteRide(request),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
    }

    private static void AssertBadRequest(IActionResult response, string message)
    {
        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal(message, badRequest.Value);
    }

    private sealed class Fixture
    {
        public Mock<IRideRepo> Repository { get; } = new(MockBehavior.Strict);
        public Mock<IHubContext<RideHub>> Hub { get; } = new(MockBehavior.Strict);
        private Mock<IHubClients> Clients { get; } = new(MockBehavior.Strict);
        private Mock<IClientProxy> Client { get; } = new(MockBehavior.Strict);
        public List<string> Events { get; } = [];
        public RideController Controller { get; }

        public Fixture()
        {
            var service = new RideService(Repository.Object, Mock.Of<IMapper>(MockBehavior.Strict),
                Mock.Of<IUserRepo>(MockBehavior.Strict), Mock.Of<IDriverRepo>(MockBehavior.Strict));
            Controller = new RideController(Hub.Object, Mock.Of<IDriverService>(MockBehavior.Strict),
                service, Mock.Of<IUserService>(MockBehavior.Strict));
        }

        public void AllowNotification(string eventName, Exception? failure = null)
        {
            Hub.SetupGet(hub => hub.Clients).Returns(Clients.Object);
            Clients.Setup(clients => clients.Group("ride-42")).Returns(Client.Object);
            Client.Setup(client => client.SendCoreAsync(eventName,
                    It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], RideId)), default))
                .Callback(() => Events.Add("notify"))
                .Returns(() => failure is null ? Task.CompletedTask : Task.FromException(failure));
        }

        public void VerifyNotification(string eventName)
        {
            Hub.VerifyGet(hub => hub.Clients, Times.Once);
            Clients.Verify(clients => clients.Group("ride-42"), Times.Once);
            Client.Verify(client => client.SendCoreAsync(eventName,
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], RideId)), default), Times.Once);
            Hub.VerifyNoOtherCalls();
            Clients.VerifyNoOtherCalls();
            Client.VerifyNoOtherCalls();
        }

        public void VerifyNoSideEffects()
        {
            Repository.Verify(repo => repo.UpdateStatus(It.IsAny<int>(), It.IsAny<RideStatus>()), Times.Never);
            Hub.VerifyNoOtherCalls();
        }
    }
}
