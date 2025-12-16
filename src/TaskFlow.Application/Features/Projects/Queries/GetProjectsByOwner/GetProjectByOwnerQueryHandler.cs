using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjectsByOwner;

/// <summary>
/// Handler for GetProjectsByOwnerQuery.
/// Returns all projects the current user owns or is a member of.
/// </summary>
public class GetProjectsByOwnerQueryHandler : IRequestHandler<GetProjectsByOwnerQuery, List<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectsByOwnerQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<List<ProjectDto>> Handle(
        GetProjectsByOwnerQuery request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get all projects where user is owner or member
        var projects = await _unitOfWork.Projects.GetProjectsByMemberAsync(
            currentUserId,
            cancellationToken);

        // Convert to DTOs
        return _mapper.Map<List<ProjectDto>>(projects);
    }
}