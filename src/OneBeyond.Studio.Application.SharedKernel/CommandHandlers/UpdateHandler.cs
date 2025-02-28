using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EnsureThat;
using FluentValidation;
using MediatR;
using OneBeyond.Studio.Application.SharedKernel.Entities.Validators;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.CommandHandlers;

/// <summary>
/// Handles <see cref="Update{TAggregateRootUpdateDto, TAggregateRoot, TAggregateRootId}"/> command
/// </summary>
/// <typeparam name="TAggregateRootUpdateDto"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public class UpdateHandler<TAggregateRootUpdateDto, TAggregateRoot, TAggregateRootId>
    : IRequestHandler<Update<TAggregateRootUpdateDto, TAggregateRoot, TAggregateRootId>, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
    where TAggregateRootId : notnull
{
    /// <summary>
    /// </summary>
    public UpdateHandler(
        IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository,
        IValidator<TAggregateRoot> validator,
        IMapper mapper)
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));
        EnsureArg.IsNotNull(validator, nameof(validator));
        EnsureArg.IsNotNull(mapper, nameof(mapper));

        RwRepository = rwRepository;
        Validator = validator;
        Mapper = mapper;
    }

    /// <summary>
    /// </summary>
    protected IRWRepository<TAggregateRoot, TAggregateRootId> RwRepository { get; }

    /// <summary>
    /// </summary>
    protected IValidator<TAggregateRoot> Validator { get; }

    /// <summary>
    /// </summary>
    protected IMapper Mapper { get; }

    /// <summary>
    /// </summary>
    public Task<TAggregateRootId> Handle(
        Update<TAggregateRootUpdateDto, TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(command, nameof(command));

        return HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// </summary>
    protected virtual async Task<TAggregateRootId> HandleAsync(
        Update<TAggregateRootUpdateDto, TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = await UpdateAggregateRootFromDtoAsync(
            command.AggregateRootId,
            command.AggregateRootUpdateDto,
            cancellationToken).ConfigureAwait(false);
        Validator.EnsureIsValid(aggregateRoot);
        await RwRepository.UpdateAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
        return command.AggregateRootId;
    }

    /// <summary>
    /// </summary>
    protected virtual async Task<TAggregateRoot> UpdateAggregateRootFromDtoAsync(
        TAggregateRootId aggregateRootId,
        TAggregateRootUpdateDto aggregateRootUpdateDto,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = await RwRepository.GetByIdAsync(aggregateRootId, cancellationToken).ConfigureAwait(false);
        aggregateRoot = Mapper.Map(aggregateRootUpdateDto, aggregateRoot);
        return aggregateRoot;
    }
}
