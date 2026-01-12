using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Application.Features.Users.Commands.RequestPasswordReset;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTest.Users;

public class RequestUserPasswordResetHandlerTest
{
    [Fact]
    public async Task RequestResetPassword_ShouldBeSuccess()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var user = new User("test", "test1", "test.test1", "Admin@123", "test@example.com");

        var forgotPasswordSettings = new ForgotPasswordSettings
        {
            Uri = "https://example.com/reset-password",
            ExpiredTimeInHour = 2,
        };

        // ---- Mocks ----
        var unitOfWorkMock = new Mock<IEfUnitOfWork>();
        var readonlyRepoMock = new Mock<IEfReadonlyRepository<User>>();
        var passwordResetRepoMock = new Mock<IEfRepository<UserPasswordReset>>();

        unitOfWorkMock.Setup(x => x.ReadonlyRepository<User>()).Returns(readonlyRepoMock.Object);
        unitOfWorkMock
            .Setup(x => x.Repository<UserPasswordReset>())
            .Returns(passwordResetRepoMock.Object);
        unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var mailServiceMock = new Mock<IMailService>();
        var translatorMock = new Mock<ITranslator<Messages>>();

        var optionsMock = new Mock<IOptions<ForgotPasswordSettings>>();
        optionsMock.Setup(x => x.Value).Returns(forgotPasswordSettings);
        translatorMock.Setup(x => x.Translate(It.IsAny<string>())).Returns("translated-message");

        readonlyRepoMock
            .Setup(x =>
                x.FindByConditionAsync(
                    It.IsAny<GetUserByEmailIncludePasswordResetRequestSpecification>(),
                    cancellationToken
                )
            )
            .ReturnsAsync(user);

        passwordResetRepoMock
            .Setup(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<UserPasswordReset>>()))
            .Verifiable();

        passwordResetRepoMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<UserPasswordReset>>()))
            .Verifiable();

        mailServiceMock
            .Setup(x =>
                x.SendWithTemplateAsync(
                    It.Is<MailTemplateData>(m =>
                        m.To.Contains(user.Email) && m.Subject == "Reset password"
                    )
                )
            )
            .ReturnsAsync(true);

        unitOfWorkMock.Setup(x => x.SaveChangesAsync(cancellationToken)).Verifiable();

        var handler = new RequestUserPasswordResetHandler(
            unitOfWorkMock.Object,
            mailServiceMock.Object,
            optionsMock.Object,
            translatorMock.Object
        );

        var command = new RequestUserPasswordResetCommand(user.Email);

        // Act
        Result<string> result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        passwordResetRepoMock.Verify(
            x => x.DeleteRangeAsync(user.PasswordResetRequests),
            Times.Once
        );

        passwordResetRepoMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<UserPasswordReset>(r =>
                        r.UserId == user.Id && !string.IsNullOrWhiteSpace(r.Token)
                    ),
                    cancellationToken
                ),
            Times.Once
        );

        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);

        mailServiceMock.Verify(
            x =>
                x.SendWithTemplateAsync(
                    It.Is<MailTemplateData>(m =>
                        m.To.Contains(user.Email) && m.Subject == "Reset password"
                    )
                ),
            Times.Once
        );
    }
}
