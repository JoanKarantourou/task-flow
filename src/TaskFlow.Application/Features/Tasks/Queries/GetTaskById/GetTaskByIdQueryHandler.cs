using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTaskById;

/// <summary>
/// Handler for GetTaskByIdQuery.
/// Retrieves a task by its ID and returns it as a DTO.
/// Query handlers are typically simpler than command handlers because they only read data.
/// Now uses AutoMapper for entity-to-DTO conversion.
/// </summary>
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor - dependencies are injected by MediatR and DI container.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work for database access</param>
    /// <param name="mapper">AutoMapper for entity-to-DTO conversion</param>
    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the query by retrieving the task from the database.
    /// Returns null if the task doesn't exist.
    /// </summary>
    /// <param name="request">The query containing the task ID</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>TaskDto if found, null if not found</returns>
    public async Task<TaskDto?> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Step 1: Retrieve the task with all related data
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(
            request.TaskId,
            cancellationToken);

        // Step 2: If task doesn't exist, return null
        if (task == null)
        {
            return null;
        }

        // Step 3: Use AutoMapper to convert entity to DTO
        // AutoMapper handles all property mapping automatically
        var taskDto = _mapper.Map<TaskDto>(task);

        return taskDto;
    }
}