using AutoMapper;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that defines all entity-to-DTO mappings.
/// AutoMapper scans for classes inheriting from Profile and applies the mappings.
/// Each mapping tells AutoMapper how to convert one type to another.
/// </summary>
/// <remarks>
/// Mapping conventions:
/// - Properties with same name are mapped automatically
/// - Custom mappings use ForMember() for complex scenarios
/// - Navigation properties need explicit mapping
/// 
/// Example automatic mapping:
/// Entity: task.Title → DTO: taskDto.Title (same name, auto-mapped)
/// 
/// Example custom mapping:
/// Entity: task.Project.Name → DTO: taskDto.ProjectName (different structure, custom mapping)
/// </remarks>
public class MappingProfile : Profile
{
    /// <summary>
    /// Constructor where we define all mappings.
    /// Called automatically by AutoMapper when it initializes.
    /// </summary>
    public MappingProfile()
    {
        // ========================================
        // Task Mappings
        // ========================================

        // TaskItem → TaskDto
        CreateMap<TaskItem, TaskDto>()
            // Most properties auto-map (same names): Id, Title, Description, Status, Priority, etc.
            .ForMember(dest => dest.ProjectName,
                opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.AssigneeName,
                opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.FullName : null))
            .ForMember(dest => dest.CommentCount,
                opt => opt.MapFrom(src => src.Comments.Count));

        // TaskItem → TaskSummaryDto
        // Used for lists where we don't need full details
        CreateMap<TaskItem, TaskSummaryDto>()
            .ForMember(dest => dest.AssigneeName,
                opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.FullName : null));

        // ========================================
        // Project Mappings
        // ========================================

        // Project → ProjectDto
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src => src.Owner.FullName))
            .ForMember(dest => dest.TotalTasks,
                opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.CompletedTasks,
                opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)))
            .ForMember(dest => dest.MemberCount,
                opt => opt.MapFrom(src => src.Members.Count));

        // Project → ProjectSummaryDto
        CreateMap<Project, ProjectSummaryDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src => src.Owner.FullName))
            .ForMember(dest => dest.TotalTasks,
                opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.CompletedTasks,
                opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)));

        // Project → ProjectDetailsDto
        // Includes related entities (Tasks and Members)
        CreateMap<Project, ProjectDetailsDto>()
            .IncludeBase<Project, ProjectDto>() // Inherit mappings from Project → ProjectDto
            .ForMember(dest => dest.Tasks,
                opt => opt.MapFrom(src => src.Tasks))
            .ForMember(dest => dest.Members,
                opt => opt.MapFrom(src => src.Members));

        // ========================================
        // User Mappings
        // ========================================

        // User → UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.FullName)); // Uses computed property from entity

        // User → UserSummaryDto
        CreateMap<User, UserSummaryDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.FullName));

        // ========================================
        // Comment Mappings
        // ========================================

        // Comment → CommentDto
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.AuthorName,
                opt => opt.MapFrom(src => src.Author.FullName))
            .ForMember(dest => dest.AuthorEmail,
                opt => opt.MapFrom(src => src.Author.Email));

        // ========================================
        // ProjectMember Mappings
        // ========================================

        // ProjectMember → ProjectMemberDto
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UserEmail,
                opt => opt.MapFrom(src => src.User.Email));
    }
}