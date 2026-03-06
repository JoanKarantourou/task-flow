using AutoMapper;
using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Mappings;
using TaskFlow.Application.Features.Projects.Commands.CreateProject;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Handlers;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly IMapper _mapper;
    private readonly CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateProjectCommandHandler(
            _unitOfWorkMock.Object,
            _mapper,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new CreateProjectCommand { Name = "Test Project" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesProjectWithCurrentUserAsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo
            .Setup(x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project p, CancellationToken _) => p);
        _unitOfWorkMock.Setup(x => x.Projects).Returns(mockProjectRepo.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Setup GetProjectWithDetailsAsync to return project with owner
        mockProjectRepo
            .Setup(x => x.GetProjectWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Project
            {
                Id = id,
                Name = "New Project",
                Description = "Description",
                Status = ProjectStatus.Active,
                OwnerId = userId,
                Owner = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@test.com" }
            });

        var command = new CreateProjectCommand
        {
            Name = "New Project",
            Description = "Description",
            Status = ProjectStatus.Active
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Project");
        result.OwnerId.Should().Be(userId);

        mockProjectRepo.Verify(x => x.AddAsync(
            It.Is<Project>(p => p.OwnerId == userId && p.Name == "New Project"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
