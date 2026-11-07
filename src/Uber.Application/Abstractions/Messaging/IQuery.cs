using Uber.Domain.Common;

namespace Uber.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
