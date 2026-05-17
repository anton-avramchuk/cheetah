using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Cheetah.FileStorage;
using Cheetah.FileStorage.S3;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.FileStorage.Tests;

public class S3FileStorageTests
{
    private static S3FileStorage Build(IAmazonS3 client) => new(
        client,
        Microsoft.Extensions.Options.Options.Create(new S3FileStorageOptions { BucketName = "test-bucket" }),
        NullLogger<S3FileStorage>.Instance);

    [Fact]
    public async Task SaveAsync_шлёт_PutObject_с_правильным_bucket_и_key()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        var sut = Build(client.Object);
        await sut.SaveAsync("a/b/file.txt", new MemoryStream(new byte[] { 1, 2 }), "text/plain",
            userMetadata: new Dictionary<string, string> { ["k"] = "v" });

        client.Verify(c => c.PutObjectAsync(
            It.Is<PutObjectRequest>(r =>
                r.BucketName == "test-bucket" &&
                r.Key == "a/b/file.txt" &&
                r.ContentType == "text/plain" &&
                r.Metadata["k"] == "v"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OpenReadAsync_преобразует_404_в_FileStorageNotFoundException()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("not found") { StatusCode = HttpStatusCode.NotFound });

        var sut = Build(client.Object);
        await Should.ThrowAsync<FileStorageNotFoundException>(() => sut.OpenReadAsync("nope").AsTask());
    }

    [Fact]
    public async Task ExistsAsync_true_если_GetObjectMetadata_успешен()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectMetadataResponse());
        var sut = Build(client.Object);

        (await sut.ExistsAsync("x")).ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_false_если_404()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("nf") { StatusCode = HttpStatusCode.NotFound });
        var sut = Build(client.Object);

        (await sut.ExistsAsync("x")).ShouldBeFalse();
    }

    [Fact]
    public void Ctor_бросает_если_BucketName_не_задан()
    {
        Should.Throw<InvalidOperationException>(() => new S3FileStorage(
            Mock.Of<IAmazonS3>(),
            Microsoft.Extensions.Options.Options.Create(new S3FileStorageOptions { BucketName = "" }),
            NullLogger<S3FileStorage>.Instance));
    }
}
