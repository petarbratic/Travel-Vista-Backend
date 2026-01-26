using AutoMapper;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.Core.UseCases;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace Explorer.Stakeholders.Tests.Unit.Achievements;

public class AchievementServiceTests
{
    private readonly Mock<IAchievementRepository> _repo = new();
    private readonly Mock<IXpEventRepository> _repoXpEvent = new();
    private readonly Mock<IMapper> _mapper = new();

    private AchievementService CreateSut() => new AchievementService(_repo.Object, _repoXpEvent.Object, _mapper.Object);

    [Fact]
    public void Create_throws_when_code_is_invalid()
    {
        var sut = CreateSut();

        var dto = new AchievementDto { Code = "NotARealCode" };

        Should.Throw<ArgumentException>(() => sut.Create(dto, touristId: 1));
    }

    [Fact]
    public void Create_throws_when_duplicate()
    {
        var sut = CreateSut();

        var dto = new AchievementDto { Code = "FirstTourCompleted" };

        _repo.Setup(r => r.Has(1, AchievementCode.FirstTourCompleted)).Returns(true);

        Should.Throw<InvalidOperationException>(() => sut.Create(dto, touristId: 1));
    }

    [Fact]
    public void Create_returns_dto_with_meta_fields()
    {
        var sut = CreateSut();

        var dto = new AchievementDto { Code = "FirstTourCompleted" };

        _repo.Setup(r => r.Has(1, AchievementCode.FirstTourCompleted)).Returns(false);

        var createdDomain = new Achievement(1, AchievementCode.FirstTourCompleted);
        _repo.Setup(r => r.Create(It.IsAny<Achievement>())).Returns(createdDomain);

        _mapper.Setup(m => m.Map<AchievementDto>(createdDomain)).Returns(new AchievementDto());

        var result = sut.Create(dto, touristId: 1);

        result.Code.ShouldBe("FirstTourCompleted");
        result.Name.ShouldBe("First Tour Completed");
        result.Description.ShouldContain("completed your first tour");
    }
}
