using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Tests.Domain;

public class EntityTests
{
    [Fact]
    public void BaseEntity_Constructor_SetsIdAndTimestamps()
    {
        var user = new User();

        user.Id.Should().NotBe(Guid.Empty);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void User_FullName_ReturnsCombinedName()
    {
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };

        user.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void TaskItem_DefaultStatus_IsTodo()
    {
        var task = new TaskItem();
        task.Status.Should().Be(TaskStatus.Todo);
    }

    [Fact]
    public void TaskItem_DefaultPriority_IsMedium()
    {
        var task = new TaskItem();
        task.Priority.Should().Be(TaskPriority.Medium);
    }

    [Fact]
    public void Project_DefaultStatus_IsActive()
    {
        var project = new Project();
        project.Status.Should().Be(ProjectStatus.Active);
    }

    [Fact]
    public void Project_TasksCollection_InitializedEmpty()
    {
        var project = new Project();
        project.Tasks.Should().NotBeNull();
        project.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void User_NavigationCollections_InitializedEmpty()
    {
        var user = new User();
        user.Projects.Should().NotBeNull().And.BeEmpty();
        user.AssignedTasks.Should().NotBeNull().And.BeEmpty();
        user.ProjectMemberships.Should().NotBeNull().And.BeEmpty();
        user.Comments.Should().NotBeNull().And.BeEmpty();
    }
}
