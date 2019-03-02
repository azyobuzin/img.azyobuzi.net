# インストール ガイド
## Docker を使用する
Docker Hub [azyobuzin/img.azyobuzi.net](https://hub.docker.com/r/azyobuzin/img.azyobuzi.net) にてイメージを公開しています。 latest タグは master ブランチの更新に追従しています。

次のコマンドで、 img.azyobuzi.net を起動します。

```sh
docker run -p 80:80/tcp -d azyobuzin/img.azyobuzi.net:latest
```

すべての画像共有サービスについて正常にレスポンスを返すように構成するには、[設定ガイド](configuration.md)に従って、環境変数を設定してください。

## ソースコードからビルドする
img.azyobuzi.net は .NET Core SDK 2.2 がインストールされた環境で、ビルドすることができます。次のコマンドで、 `/path/to/output` にビルド結果を出力します。

```sh
cd ImgAzyobuziNet
dotnet publish --output /path/to/output
```

次のコマンドで、ビルド結果から img.azyobuzi.net を起動することができます。

```sh
ASPNETCORE_URLS=http://+:80 dotnet ImgAzyobuziNet.dll
```

すべての画像共有サービスについて正常にレスポンスを返すように構成するには、[設定ガイド](configuration.md)に従って、設定を変更する必要があります。設定は、`appsettings.json`、環境変数、コマンドライン引数より指定することができます。

## Azure Functions にデプロイする
img.azyobuzi.net を Azure Functions にデプロイすることができます。もっとも簡単にデプロイを行うには、 Visual Studio の発行ツールを使用します。コマンドラインで行うには Azure Functions Core Tools を使用します。詳しくは、[Azure のドキュメント](https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-run-local#publish)をご覧ください。

すべての画像共有サービスについて正常にレスポンスを返すように構成するには、[設定ガイド](configuration.md)に従って、設定を変更する必要があります。設定は、「アプリケーション設定」より指定でき、環境変数と同じ指定方法です。
