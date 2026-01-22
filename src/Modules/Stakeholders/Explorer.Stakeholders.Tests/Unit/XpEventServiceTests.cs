using AutoMapper;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.Core.UseCases;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace Explorer.Stakeholders.Tests.Unit.XpEvents;

public class XpEventServiceTests
{
    private readonly Mock<IXpEventRepository> _repo = new();
    private readonly Mock<IMapper> _mapper = new();

    private XpEventService CreateSut() => new XpEventService(_repo.Object, _mapper.Object);

    [Fact]
    public void Create_throws_when_amount_is_zero_or_less()
    {
        var sut = CreateSut();

        var dto = new XpEventDto { Type = "TourCompleted", Amount = 0, SourceEntityId = 1 };

        Should.Throw<ArgumentException>(() => sut.Create(dto, touristId: 1));
    }

    [Fact]
    public void Create_throws_when_type_is_invalid()
    {
        var sut = CreateSut();

        var dto = new XpEventDto { Type = "NotAType", Amount = 10, SourceEntityId = 1 };

        Should.Throw<ArgumentException>(() => sut.Create(dto, touristId: 1));
    }

    [Fact]
    public void Create_throws_when_duplicate_exists()
    {
        var sut = CreateSut();

        var dto = new XpEventDto { Type = "TourCompleted", Amount = 10, SourceEntityId = 123 };

        _repo.Setup(r => r.Exists(1, XpEventType.TourCompleted, 123)).Returns(true);

        Should.Throw<InvalidOperationException>(() => sut.Create(dto, touristId: 1));
    }

    [Fact]
    public void Create_returns_dto_with_description_and_string_type()
    {
        var sut = CreateSut();

        var dto = new XpEventDto { Type = "TourCompleted", Amount = 25, SourceEntityId = 99 };

        _repo.Setup(r => r.Exists(1, XpEventType.TourCompleted, 99)).Returns(false);

        var createdDomain = new XpEvent(1, XpEventType.TourCompleted, 25, 99);
        _repo.Setup(r => r.Create(It.IsAny<XpEvent>())).Returns(createdDomain);

        // Mapper vrati "osnovni" dto, servis dopuni Type + Description
        _mapper.Setup(m => m.Map<XpEventDto>(createdDomain)).Returns(new XpEventDto
        {
            Amount = 25,
            SourceEntityId = 99
        });

        var result = sut.Create(dto, touristId: 1);

        result.Type.ShouldBe("TourCompleted");
        result.Description.ShouldContain("Tour completed");
        result.Amount.ShouldBe(25);
        result.SourceEntityId.ShouldBe(99);
    }
}
