namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Service interface for accessing the current authenticated user's information.
/// Extracts user details from JWT claims in the HTTP context.
/// </summary>
/// <remarks>
/// This service is used by handlers and controllers to identify who is making the request.
/// It reads claims from the JWT token that was validated by the authentication middleware.
/// 
/// Typical usage:
/// - Check if user is authenticated
/// - Get user ID for authorization checks
/// - Get user info for audit logging
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// Returns null if no user is authenticated (anonymous request).
    /// </summary>
    /// <example>
    /// var userId = _currentUserService.UserId;
    /// if (userId == null)
    /// {
    ///     throw new UnauthorizedAccessException("User must be logged in");
    /// }
    /// 
    /// // Use userId to filter data
    /// var myProjects = await _unitOfWork.Projects.GetProjectsByOwnerAsync(userId.Value);
    /// </example>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the email address of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    /// <example>
    /// var email = _currentUserService.Email;
    /// _logger.LogInformation($"User {email} accessed this resource");
    /// </example>
    string? Email { get; }

    /// <summary>
    /// Gets the full name of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    /// <example>
    /// var name = _currentUserService.FullName;
    /// var notification = $"Welcome back, {name}!";
    /// </example>
    string? FullName { get; }

    /// <summary>
    /// Checks if a user is currently authenticated.
    /// Returns true if UserId is not null.
    /// </summary>
    /// <example>
    /// if (!_currentUserService.IsAuthenticated)
    /// {
    ///     return Unauthorized("You must be logged in");
    /// }
    /// </example>
    bool IsAuthenticated { get; }
}

// ============================================
// How CurrentUserService Works
// ============================================
//
// 1. User logs in → Receives JWT token
// 2. Client sends JWT in Authorization header:
//    Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
//
// 3. ASP.NET Core authentication middleware:
//    - Validates the JWT signature
//    - Extracts claims from token payload
//    - Creates ClaimsPrincipal
//    - Stores in HttpContext.User
//
// 4. CurrentUserService reads from HttpContext.User:
//    - UserId from "sub" or "nameid" claim
//    - Email from "email" claim
//    - FullName from "name" claim
//
// 5. Handlers use CurrentUserService:
//    var userId = _currentUserService.UserId;
//    // Use userId for business logic
//
// ============================================
// JWT Claims Explained
// ============================================
//
// Standard JWT claims (from token payload):
// {
//   "sub": "123e4567-e89b-12d3-a456-426614174000",  // Subject (User ID)
//   "email": "john@example.com",                     // Email
//   "name": "John Doe",                              // Full name
//   "nbf": 1638360000,                               // Not Before timestamp
//   "exp": 1638363600,                               // Expiration timestamp
//   "iat": 1638360000                                // Issued At timestamp
// }
//
// These claims become available via:
// HttpContext.User.Claims
//
// CurrentUserService maps:
// - "sub" claim → UserId
// - "email" claim → Email
// - "name" claim → FullName
//
// ============================================
// Usage Examples
// ============================================
//
// Example 1: Authorization check in handler
// public class CreateTaskCommandHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly IUnitOfWork _unitOfWork;
//     
//     public async Task<TaskDto> Handle(CreateTaskCommand request, ...)
//     {
//         // Check if user owns the project
//         var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId);
//         if (project.OwnerId != _currentUser.UserId)
//         {
//             throw new UnauthorizedAccessException("You don't own this project");
//         }
//         
//         // Create task...
//     }
// }
//
// Example 2: Audit logging
// public class DeleteTaskCommandHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly ILogger _logger;
//     
//     public async Task Handle(DeleteTaskCommand request, ...)
//     {
//         _logger.LogInformation(
//             "User {Email} ({UserId}) deleted task {TaskId}",
//             _currentUser.Email,
//             _currentUser.UserId,
//             request.TaskId);
//         
//         // Delete task...
//     }
// }
//
// Example 3: Filter data by current user
// public class GetMyProjectsQueryHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly IUnitOfWork _unitOfWork;
//     
//     public async Task<List<ProjectDto>> Handle(GetMyProjectsQuery request, ...)
//     {
//         if (!_currentUser.IsAuthenticated)
//         {
//             return new List<ProjectDto>(); // Return empty for anonymous
//         }
//         
//         var projects = await _unitOfWork.Projects
//             .GetProjectsByOwnerAsync(_currentUser.UserId.Value);
//         
//         return _mapper.Map<List<ProjectDto>>(projects);
//     }
// }
//
// ============================================
// Dependency Injection
// ============================================
//
// CurrentUserService is registered as Scoped:
// services.AddScoped<ICurrentUserService, CurrentUserService>();
//
// Scoped = One instance per HTTP request
// - HttpContext is available throughout the request
// - User info is consistent for the entire request
// - Disposed when request completes
//
// ============================================
// Security Considerations
// ============================================
//
// Always validate authorization in handlers:
// - Don't trust client to send correct user ID
// - Use _currentUser.UserId from JWT token
// - Verify user has permission for the action
//
// Example: Preventing unauthorized access
// var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
// if (task.AssigneeId != _currentUser.UserId && 
//     task.Project.OwnerId != _currentUser.UserId)
// {
//     throw new UnauthorizedAccessException();
// }
