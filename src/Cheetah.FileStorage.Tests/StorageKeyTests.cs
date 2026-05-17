using Cheetah.FileStorage;
using Shouldly;

namespace Cheetah.FileStorage.Tests;

public class StorageKeyTests
{
    [Theory]
    [InlineData("file.pdf")]
    [InlineData("attachments/2026/05/file.pdf")]
    [InlineData("a-b_c.d/x")]
    public void Validate_пропускает_корректные(string key)
        => Should.NotThrow(() => StorageKey.Validate(key));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/leading-slash")]
    [InlineData("trailing-slash/")]
    [InlineData("double//slash")]
    [InlineData("../escape")]
    [InlineData("a/./b")]
    [InlineData("a/../b")]
    [InlineData("with space.txt")]
    [InlineData("with;semicolon")]
    public void Validate_отвергает_небезопасные(string key)
        => Should.Throw<ArgumentException>(() => StorageKey.Validate(key));
}
