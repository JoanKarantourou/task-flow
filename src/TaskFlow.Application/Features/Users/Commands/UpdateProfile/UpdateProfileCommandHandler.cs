using AutoMapper;
using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Handler for UpdateProfileCommand.
/// Updates the current user's profile.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IGenericRepository<User> userRepository,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        UpdateProfileCommand request,
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

        // Check if email is being changed and if it's already taken
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var emailExists = await _userRepository.AnyAsync(
                u => u.Email == request.Email && u.Id != currentUserId,
                cancellationToken);

            if (emailExists)
            {
                throw new ArgumentException("Email address is already in use");
            }

            user.Email = request.Email;
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        // Update timestamp
        user.UpdatedAt = DateTime.UtcNow;

        // Save changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
