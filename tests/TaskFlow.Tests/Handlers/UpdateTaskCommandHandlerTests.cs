using AutoMapper;
using FluentAssertions;
using MassTransit;
using Moq;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Features.Tasks.Commands.UpdateTask;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Tests.Handlers;

public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock = new();
    private readonly IMapper _mapper;
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new UpdateTaskCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _publishEndpointMock.Object,
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);

        var command = new UpdateTaskCommand { TaskId = Guid.NewGuid(), Title = "Updated" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        var command = new UpdateTaskCommand { TaskId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_StatusChanged_PublishesTaskStatusChangedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Existing Task",
            ProjectId = projectId,
            Status = TaskStatus.Todo,
            Priority = TaskPriority.Medium
        };

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var project = new Project { Id = projectId, Name = "Project", OwnerId = userId };
        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        var user = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            Status = TaskStatus.InProgress
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TaskStatus.InProgress);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<TaskStatusChangedEvent>(e =>
                e.TaskId == taskId &&
                e.OldStatus == TaskStatus.Todo &&
                e.NewStatus == TaskStatus.InProgress),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AssigneeChanged_PublishesTaskAssignedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newAssigneeId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Task",
            ProjectId = projectId,
            Status = TaskStatus.Todo,
            AssigneeId = null
        };

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTask);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var project = new Project { Id = projectId, Name = "Project", OwnerId = userId };
        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        var currentUser = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        var newAssignee = new User { Id = newAssigneeId, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(newAssigneeId, It.IsAny<CancellationToken>())).ReturnsAsync(newAssignee);

        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            AssigneeId = newAssigneeId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<TaskAssignedEvent>(e =>
                e.TaskId == taskId &&
                e.AssigneeId == newAssigneeId &&
                e.AssigneeEmail == "jane@test.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
