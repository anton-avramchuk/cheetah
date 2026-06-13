using Cheetah.Backend.Grpc.Abstractions;
using Crm.Customer.Application;
using Crm.Customer.Application.Commands;
using Crm.Customer.Application.Queries;

namespace Crm.Customer.Api.Grpc;

// Классы-маркеры: связывают proto-методы с CQRS. По ним gRPC-генератор создаёт CustomerGrpcService —
// наследник CustomerGrpc.CustomerGrpcBase с плумбингом к IDispatcher.

public sealed class GetCustomerByIdGrpcMethod
    : GrpcQueryOrNotFound<GetCustomerByIdGrpcRequest, GetCustomerByIdQuery, CustomerModel?, CustomerGrpcReply>;

public sealed class CreateCustomerGrpcMethod
    : GrpcCommandWithResult<CreateCustomerGrpcRequest, CreateCustomerCommand, System.Guid, CreateCustomerGrpcReply>;
