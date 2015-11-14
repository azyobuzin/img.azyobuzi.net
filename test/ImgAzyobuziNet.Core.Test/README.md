# ImgAzyobuziNet.Core テストツール
## 実行方法
```
dnx test [テストの種類]
```

テストの種類
* static 基本的な単体テスト
* network インターネットに接続してサービスの生存確認

省略すると static になります。

## テストの書き方
ImgAzyobuziNet.Core 内で `TestMethodAttribute` を指定したメソッドが実行されます。

```csharp
class TestClass
{
	[TestMethod(TestType.Static)]
	private static void StaticTest()
	{
		Ok();
	}
	
	[TestMethod(TestType.Network)]
	private static async Task NetworkTest()
	{
		await OkAsync().ConfigureAwait(false);
	}
}
```
