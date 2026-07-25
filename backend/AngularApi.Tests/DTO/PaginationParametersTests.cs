using AngularApi.DTO;
using FluentAssertions;

namespace AngularApi.Tests.DTO;

public class PaginationParametersTests
{
    [Fact]
    public void PageSize_DefaultsToTwenty_WhenNotProvided()
    {
        var pagination = new PaginationParameters();

        pagination.PageSize.Should().Be(20);
        pagination.Page.Should().Be(1);
    }

    [Fact]
    public void PageSize_CapsAtOneHundred()
    {
        var pagination = new PaginationParameters { PageSize = 500 };

        pagination.PageSize.Should().Be(100);
    }

    [Fact]
    public void Page_NormalizesToMinimumOne()
    {
        var pagination = new PaginationParameters { Page = 0 };

        pagination.Page.Should().Be(1);
    }
}
