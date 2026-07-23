using AngularApi.Services;
using AngularApi.Services.Interfaces;
using FluentAssertions;

namespace AngularApi.Tests.Services;

public class EmailServiceAsyncTests
{
    [Fact]
    public async Task SendEmailAsync_IsAwaitableWithoutBlockingCaller()
    {
        var tcs = new TaskCompletionSource<bool>();
        var emailService = new AsyncTestEmailService(tcs.Task);

        var sendTask = emailService.SendEmailAsync(new Message(["test@example.com"], "Subject", "Body"));

        sendTask.IsCompleted.Should().BeFalse();
        tcs.SetResult(true);
        await sendTask;
        sendTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    private sealed class AsyncTestEmailService(Task delayTask) : IEmailService
    {
        public async Task SendEmailAsync(Message message)
        {
            await delayTask;
        }
    }
}
