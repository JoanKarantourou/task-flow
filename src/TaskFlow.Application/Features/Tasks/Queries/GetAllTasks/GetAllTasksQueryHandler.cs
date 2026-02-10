using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Tasks.Queries.GetAllTasks;

/// <summary>
/// Handler for GetAllTasksQuery.
/// Returns paginated tasks from projects the user has access to.
/// </summary>
public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, PagedResult<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAllTasksQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<TaskDto>> Handle(
        GetAllTasksQuery request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get paginated tasks
        var (items, totalCount) = await _unitOfWork.Tasks.GetTasksPagedAsync(
            request.Parameters,
            currentUserId,
            cancellationToken);

        // Map to DTOs
        var taskDtos = _mapper.Map<List<TaskDto>>(items);

        return new PagedResult<TaskDto>
        {
            Items = taskDtos,
            TotalCount = totalCount,
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize
        };
    }
}
