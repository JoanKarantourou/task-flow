using FluentAssertions;
using Moq;
using TaskFlow.Application.Features.Tasks.Commands.DeleteTask;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Tests.Handlers;

public class DeleteTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly DeleteTaskCommandHandler _handler;

    public DeleteTaskCommandHandlerTests()
    {
        _handler = new DeleteTaskCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new DeleteTaskCommand { TaskId = Guid.NewGuid() };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetTaskWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);

        var command = new DeleteTaskCommand { TaskId = Guid.NewGuid() };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserIsProjectOwner_DeletesTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task to delete",
            ProjectId = projectId,
            Status = TaskStatus.Todo,
            AssigneeId = Guid.NewGuid() // someone else is assignee
        };

        var project = new Project { Id = projectId, Name = "Project", OwnerId = userId };

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetTaskWithDetailsAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);

        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteTaskCommand { TaskId = taskId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        mockTaskRepo.Verify(x => x.Delete(task), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserIsAssignee_DeletesTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task to delete",
            ProjectId = projectId,
            Status = TaskStatus.Todo,
            AssigneeId = userId // current user is assignee
        };

        var project = new Project { Id = projectId, Name = "Project", OwnerId = Guid.NewGuid() }; // different owner

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetTaskWithDetailsAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);

        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteTaskCommand { TaskId = taskId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserHasNoPermission_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task",
            ProjectId = projectId,
            AssigneeId = Guid.NewGuid() // different user is assignee
        };

        var project = new Project { Id = projectId, Name = "Project", OwnerId = Guid.NewGuid() }; // different owner

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetTaskWithDetailsAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);

        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        var command = new DeleteTaskCommand { TaskId = taskId };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
