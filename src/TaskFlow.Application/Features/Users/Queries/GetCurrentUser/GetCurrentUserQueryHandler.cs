using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Users.Queries.GetCurrentUser;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// Returns the current authenticated user's profile.
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IGenericRepository<User> userRepository,
        IMapper mapper)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var currentUserId = _currentUserService.UserId.Value;

        // Get the user
        var user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);

        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        return _mapper.Map<UserDto>(user);
    }
}
