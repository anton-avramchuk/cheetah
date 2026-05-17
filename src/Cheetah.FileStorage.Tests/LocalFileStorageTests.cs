using System.Text;
using Cheetah.FileStorage;
using Cheetah.FileStorage.Local;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.FileStorage.Tests;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _root;
    private readonly LocalFileStorage _sut;

    public LocalFileStorageTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"cheetah-fs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
        _sut = new LocalFileStorage(
            Microsoft.Extensions.Options.Options.Create(new LocalFileStorageOptions { RootPath = _root }),
            NullLogger<LocalFileStorage>.Instance);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public async Task Save_Read_Metadata_round_trip()
    {
        var bytes = Encoding.UTF8.GetBytes("hello world");
        await _sut.SaveAsync("docs/x.txt", new MemoryStream(bytes), "text/plain",
            userMetadata: new Dictionary<string, string> { ["author"] = "ant" });

        (await _sut.ExistsAsync("docs/x.txt")).ShouldBeTrue();

        await using var stream = await _sut.OpenReadAsync("docs/x.txt");
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).ShouldBe("hello world");

        var meta = await _sut.GetMetadataAsync("docs/x.txt");
        meta.ShouldNotBeNull();
        meta!.Size.ShouldBe(bytes.Length);
        meta.ContentType.ShouldBe("text/plain");
        meta.UserMetadata.ShouldNotBeNull();
        meta.UserMetadata!["author"].ShouldBe("ant");
    }

    [Fact]
    public async Task Delete_идемпотентен_и_удаляет_sidecar()
    {
        await _sut.SaveAsync("a.bin", new MemoryStream(new byte[] { 1, 2, 3 }), "application/octet-stream");
        await _sut.DeleteAsync("a.bin");
        (await _sut.ExistsAsync("a.bin")).ShouldBeFalse();

        // повторный delete не падает
        await _sut.DeleteAsync("a.bin");

        // sidecar тоже удалён
        File.Exists(Path.Combine(_root, "a.bin.meta.json")).ShouldBeFalse();
    }

    [Fact]
    public async Task OpenRead_бросает_FileStorageNotFoundException()
        => await Should.ThrowAsync<FileStorageNotFoundException>(
            () => _sut.OpenReadAsync("nope.bin").AsTask());

    [Fact]
    public async Task Path_traversal_отвергается()
    {
        await Should.ThrowAsync<ArgumentException>(
            () => _sut.SaveAsync("../escape.txt", new MemoryStream(), "text/plain").AsTask());
    }

    [Fact]
    public async Task Save_перезаписывает_существующий_файл()
    {
        await _sut.SaveAsync("k.txt", new MemoryStream(Encoding.UTF8.GetBytes("v1")), "text/plain");
        await _sut.SaveAsync("k.txt", new MemoryStream(Encoding.UTF8.GetBytes("v2-longer")), "text/plain");

        await using var stream = await _sut.OpenReadAsync("k.txt");
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).ShouldBe("v2-longer");
    }
}
