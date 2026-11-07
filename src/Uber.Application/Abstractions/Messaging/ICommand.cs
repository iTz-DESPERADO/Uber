using Uber.Domain.Common;

namespace Uber.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
