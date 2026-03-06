using AutoMapper;
using FluentAssertions;
using MassTransit;
using Moq;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Tests.Handlers;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock = new();
    private readonly IMapper _mapper;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateTaskCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _publishEndpointMock.Object,
            _currentUserServiceMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTaskAndPublishesEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo
            .Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };
        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        var creator = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(creator);

        var command = new CreateTaskCommand
        {
            Title = "New Task",
            Description = "Task description",
            ProjectId = projectId,
            Priority = TaskPriority.High
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.Priority.Should().Be(TaskPriority.High);
        result.Status.Should().Be(TaskStatus.Todo);

        mockTaskRepo.Verify(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<TaskCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new CreateTaskCommand
        {
            Title = "New Task",
            ProjectId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAssignee_PublishesEventWithAssigneeInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var mockTaskRepo = new Mock<ITaskRepository>();
        mockTaskRepo
            .Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);
        _unitOfWorkMock.Setup(x => x.Tasks).Returns(mockTaskRepo.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };
        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);

        var creator = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" };
        var assignee = new User { Id = assigneeId, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(creator);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(assigneeId, It.IsAny<CancellationToken>())).ReturnsAsync(assignee);

        var command = new CreateTaskCommand
        {
            Title = "Assigned Task",
            ProjectId = projectId,
            AssigneeId = assigneeId
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<TaskCreatedEvent>(e =>
                e.AssigneeId == assigneeId &&
                e.AssigneeEmail == "jane@test.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
