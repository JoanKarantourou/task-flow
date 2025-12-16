using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// Service for accessing the current authenticated user's information from HTTP context.
/// Reads user claims from the JWT token that was validated by authentication middleware.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Constructor - DI injects HttpContextAccessor.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            // Get the "sub" (subject) claim from JWT token
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User
                .FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    /// <summary>
    /// Gets the email address of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    public string? Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value
                ?? _httpContextAccessor.HttpContext?.User
                .FindFirst("email")?.Value;
        }
    }

    /// <summary>
    /// Gets the full name of the currently authenticated user.
    /// Returns null if no user is authenticated.
    /// </summary>
    public string? FullName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Name)?.Value
                ?? _httpContextAccessor.HttpContext?.User
                .FindFirst("name")?.Value;
        }
    }

    /// <summary>
    /// Checks if a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}

// ============================================
// How CurrentUserService Works
// ============================================
//
// Request Flow:
// 1. Client sends request with JWT token:
//    Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
//
// 2. ASP.NET Core Authentication Middleware:
//    - Extracts token from Authorization header
//    - Validates token signature
//    - Validates issuer, audience, expiration
//    - Extracts claims from token payload
//    - Creates ClaimsPrincipal from claims
//    - Sets HttpContext.User = ClaimsPrincipal
//
// 3. CurrentUserService reads from HttpContext.User:
//    - UserId from "sub" or ClaimTypes.NameIdentifier
//    - Email from "email" or ClaimTypes.Email
//    - FullName from "name" or ClaimTypes.Name
//
// 4. Handlers/Controllers use CurrentUserService:
//    var userId = _currentUserService.UserId;
//
// ============================================
// Claims Mapping
// ============================================
//
// JWT Token Claims → HttpContext Claims
//
// Token has:                    Maps to:
// "sub": "user-guid"     →     ClaimTypes.NameIdentifier or "sub"
// "email": "user@..."    →     ClaimTypes.Email or "email"
// "name": "John Doe"     →     ClaimTypes.Name or "name"
//
// We check both standard claim types and JWT claim names
// because different libraries use different conventions.
//
// ============================================
// HttpContextAccessor Explained
// ============================================
//
// HttpContext:
// - Contains information about the current HTTP request
// - Available throughout the request pipeline
// - Provides User, Request, Response, Session, etc.
//
// HttpContextAccessor:
// - Service that provides access to HttpContext
// - Registered as Singleton in DI
// - Uses AsyncLocal to provide correct context per request
//
// Why use accessor instead of direct access?
// - Services don't have direct access to HttpContext
// - Accessor makes it available anywhere via DI
// - Thread-safe (uses AsyncLocal storage)
//
// Registration in Program.cs:
// builder.Services.AddHttpContextAccessor();
//
// ============================================
// Usage Examples in Handlers
// ============================================
//
// Example 1: Authorization Check
// public class CreateTaskCommandHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly IUnitOfWork _unitOfWork;
//     
//     public async Task<TaskDto> Handle(CreateTaskCommand request, ...)
//     {
//         // Get project to check ownership
//         var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId);
//         
//         // Check if current user owns the project
//         if (project.OwnerId != _currentUser.UserId)
//         {
//             throw new UnauthorizedAccessException(
//                 "You don't have permission to create tasks in this project");
//         }
//         
//         // Create task...
//     }
// }
//
// Example 2: Auto-fill Current User
// public class CreateProjectCommandHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly IUnitOfWork _unitOfWork;
//     
//     public async Task<ProjectDto> Handle(CreateProjectCommand request, ...)
//     {
//         // Automatically set owner to current user
//         var project = new Project
//         {
//             Name = request.Name,
//             Description = request.Description,
//             OwnerId = _currentUser.UserId.Value, // Current user is owner
//         };
//         
//         await _unitOfWork.Projects.AddAsync(project);
//         await _unitOfWork.SaveChangesAsync();
//         
//         return _mapper.Map<ProjectDto>(project);
//     }
// }
//
// Example 3: Filter Data by User
// public class GetMyProjectsQueryHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly IUnitOfWork _unitOfWork;
//     
//     public async Task<List<ProjectDto>> Handle(GetMyProjectsQuery request, ...)
//     {
//         // Get only projects owned by current user
//         var projects = await _unitOfWork.Projects
//             .GetProjectsByOwnerAsync(_currentUser.UserId.Value);
//         
//         return _mapper.Map<List<ProjectDto>>(projects);
//     }
// }
//
// Example 4: Audit Logging
// public class DeleteTaskCommandHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     private readonly ILogger _logger;
//     
//     public async Task Handle(DeleteTaskCommand request, ...)
//     {
//         _logger.LogInformation(
//             "User {Email} (ID: {UserId}) is deleting task {TaskId}",
//             _currentUser.Email,
//             _currentUser.UserId,
//             request.TaskId);
//         
//         // Delete task...
//     }
// }
//
// ============================================
// Handling Unauthenticated Requests
// ============================================
//
// For endpoints without [Authorize] attribute:
// - HttpContext.User exists but has no claims
// - UserId, Email, FullName return null
// - IsAuthenticated returns false
//
// Example:
// public class PublicQueryHandler
// {
//     private readonly ICurrentUserService _currentUser;
//     
//     public async Task<List<ProjectDto>> Handle(...)
//     {
//         if (_currentUser.IsAuthenticated)
//         {
//             // Return user's private projects
//             return await GetPrivateProjects(_currentUser.UserId.Value);
//         }
//         else
//         {
//             // Return only public projects
//             return await GetPublicProjects();
//         }
//     }
// }
//
// ============================================
// Security Notes
// ============================================
//
// 1. Always check IsAuthenticated:
//    if (!_currentUser.IsAuthenticated)
//    {
//        throw new UnauthorizedAccessException();
//    }
//
// 2. Never trust client-sent user IDs:
//    // BAD: Using user ID from request
//    var project = await GetByIdAsync(request.UserId);
//    
//    // GOOD: Using user ID from JWT token
//    var project = await GetByIdAsync(_currentUser.UserId.Value);
//
// 3. Use [Authorize] on controllers/endpoints:
//    [Authorize]
//    public class TasksController : ControllerBase
//    {
//        // All methods require authentication
//    }
//
// 4. Validate authorization in handlers:
//    // Token validation = Authentication (who are you?)
//    // Permission checks = Authorization (can you do this?)
//    
//    if (resource.OwnerId != _currentUser.UserId)
//    {
//        throw new UnauthorizedAccessException();
//    }
//
// ============================================
// Dependency Injection
// ============================================
//
// Register in Program.cs:
// builder.Services.AddHttpContextAccessor();
// builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
//
// Scoped lifetime:
// - One instance per HTTP request
// - HttpContext is request-scoped
// - CurrentUserService reads from HttpContext
// - Must be Scoped to match HttpContext lifetime
