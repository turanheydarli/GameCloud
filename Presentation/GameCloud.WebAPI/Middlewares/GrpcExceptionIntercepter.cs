using GameCloud.Application.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Security.Authentication;
using FluentValidation;

namespace GameCloud.WebAPI.Middlewares
{
    public class GrpcExceptionInterceptor : Interceptor
    {
        private readonly ILogger<GrpcExceptionInterceptor> _logger;

        public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during gRPC call: {Method}", context.Method);
                throw MapToGrpcException(ex, context);
            }
        }

        private RpcException MapToGrpcException(Exception exception, ServerCallContext context)
        {
            var status = StatusCode.Internal;
            var message = "An unexpected error occurred";
            var metadata = new Metadata();

            metadata.Add("request-id", context.GetHttpContext().TraceIdentifier);

            // Map specific exception types to appropriate gRPC status codes
            switch (exception)
            {
                case InvalidCredentialException:
                    status = StatusCode.Unauthenticated;
                    message = exception.Message;
                    break;

                case ForbiddenException:
                    status = StatusCode.PermissionDenied;
                    message = exception.Message;
                    break;

               

                case InvalidUserClaimsException:
                    status = StatusCode.InvalidArgument;
                    message = exception.Message;
                    break;


                case ValidationException validationEx:
                    status = StatusCode.InvalidArgument;
                    message = validationEx.Message;
                    // Add validation errors to metadata
                    if (validationEx.Errors != null)
                    {
                        foreach (var error in validationEx.Errors)
                        {
                            metadata.Add($"validation-error-{error.PropertyName}", error.ErrorMessage);
                        }
                    }
                    break;

                case NotFoundException:
                    status = StatusCode.NotFound;
                    message = exception.Message;
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception");
                    status = StatusCode.Internal;
                    message = "An internal error occurred";
                    break;
            }

            metadata.Add("error-type", exception.GetType().Name);

            return new RpcException(new Status(status, message), metadata);
        }
    }
}