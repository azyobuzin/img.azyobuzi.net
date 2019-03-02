# 設定ガイド
## API キー設定
API キーが必要なサービスを使用するには、設定を行う必要があります。キーが設定されていない場合、該当サービスの展開を行おうとすると `ImgAzyobuziNet.Core.NotConfiguredException` がスローされます。

| 設定名 | 説明 |
| ------ | ---- |
| ApiKeys:FlickrApiKey | Flickr の API キーです。 [App Garden](https://www.flickr.com/services/apps/create/) より登録することができます。 Secret は必要ありません。 |
| ApiKeys:MobypictureDeveloperKey | Mobypicture の API キーです。[開発者用ページ](http://www.mobypicture.com/apps/my)から登録することができます。 |
| ApiKeys:TinamiApiKey | TINAMI の API キーです。[こちらの申請フォーム](http://www.tinami.com/api/key/form)から登録することができます。この API キーは、画像の URI の一部として公開されることに注意してください。 |
| ApiKeys:TwitterConsumerKey | Twitter の API キーです。権限は Read-only で十分です。[申請フォーム](https://developer.twitter.com/en/apps/create)から登録することができますが、審査が必要なことに注意してください。 |
| ApiKeys:TwitterConsumerSecret | Twitter の API キー(API key secret)です。 |
| ApiKeys:TwitterAccessToken | オプションです。ここで [Application-only 認証](https://developer.twitter.com/en/docs/basics/authentication/overview/application-only)のアクセストークンを設定すると、そのトークンを使用して API を呼び出します。指定しなかった場合は、初めて Twitter の展開を行うときに、自動でトークンを取得します。 |

## キャッシュ
画像の URI を取得するために、サービスにアクセスする必要があるリゾルバーでは、アクセスした結果をキャッシュすることがあります。キャッシュの保存先を設定するには以下の設定を変更してください。デフォルトでは、メモリー内に保存され、1日アクセスがなかったものは削除されます。

| 設定名 | 説明 |
| ------ | ---- |
| ResolverCache:Type | `None`/`Memory`/`AzureTableStorage`。デフォルトは `Memory` |
| ResolverCache:ExpirationSeconds | キャッシュの有効期間を秒数で指定します。デフォルトは 86400 秒です。 `null` を指定すると有効期限を無限にできますが、 ASP.NET Core の仕様上、外部から `null` を指定することはできないので、 `null` を指定したい場合はソースコードから変更する必要があります。 |
| ResolverCache:AzureTableStorageConnectionString | `ResolverCache:Type` に `AzureTableStorage` を指定したときに必須の設定です。 Azure ポータルに表示される接続文字列を指定したください。エミュレータを使用する場合は `UseDevelopmentStorage=true` を指定します。 |
| ResolverCache:AzureTableStorageTableName | `ResolverCache:Type` に `AzureTableStorage` を指定したときに必須の設定です。テーブル名を指定してください。ここで指定したテーブル名のテーブルは、必要に応じて自動で作成されます。 |

- **None**: キャッシュを保存しません。
- **Memory**: メモリー内に保存します。
- **AzureTableStorage**: [Azure Table Storage](https://azure.microsoft.com/ja-jp/services/storage/tables/) に保存します。

### Azure Table Storage のキャッシュと Azure Functions の設定
Azure Table Storage をキャッシュの保存先に設定して、通常の（Functions ではない） img.azyobuzi.net を使用している場合、 `ResolverCache:ExpirationSeconds` の設定に関わらず、キャッシュは削除されません。 Azure Functions を使用している場合、 1 時間ごとにキャッシュを走査し、最終更新日時がキャッシュ有効期間より古い場合、削除を行います。

以上より、 Azure Table Storage をキャッシュの保存先とする場合、デプロイ方法に関わらず、 Azure Functions へのデプロイを行い、タイマージョブでキャッシュを削除することをおすすめします。

逆に、 Azure Functions を使用していて、キャッシュの保存先が Azure Table Storage 以外の場合、キャッシュを削除するためのタイマージョブは無駄になります。そのため、 Azure ポータルから `DeleteExpiredCache` 関数を無効化することをおすすめします。

## appsettings.json を使用する
appsettings.json または、 appsettings.環境名.json では、設定名の「:」で区切られた部分が JSON オブジェクトに対応します。例えば、 `ApiKeys:FlickrApiKey` を設定すると、次のような JSON になります。

```json
{
    "ApiKeys": {
        "FlickrApiKey": "API KEY"
    }
}
```

サンプル設定が ImgAzyobuziNet/appsettings.json にありますので、確認してください。

## 環境変数、 Azure Functions のアプリケーション設定
環境変数では、設定名を環境変数名として、値を設定することができます。「:」を環境変数名として使用できない環境では、代わりに「__」（アンダースコア 2 つ）を使用することができます。

詳しくは、 [ASP.NET Core のドキュメント](https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2#environment-variables-configuration-provider)をご覧ください。
