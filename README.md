# img.azyobuzi.net #
[http://img.azyobuzi.net/](http://img.azyobuzi.net/) のソースコードです。

不具合報告など Issue または Twitter @azyobuzin に投げていただけるとうれしいです。

## ブランチについて ##
masterは本番サーバーで稼働中のもの、devは機能追加・バグ修正のテスト中または公開待機中のものです。

## ミラーサイトの作り方 ##
- Apache ・ Python 2.7 ・ MySQL 5 が実行できるサーバーを用意する
- ライブラリをインストールする
	- [python-oauth2](http://pypi.python.org/pypi/oauth2)
	- [MySQL-python](http://pypi.python.org/pypi/MySQL-python)
- src の中身を放り込む
- mirroring/private_constant.py を resolvers ディレクトリに突っ込み、環境に合わせて dbName と dbPassword を書き換える
- 環境に合わせて、 Python のパスや、 .htaccess を書き換える
- dbsnapshot.sql を MySQL で実行

これで多分動くと思います。

できるだけ、 DB のデータを同期したいと考えていますので、ミラーサイトを作成された場合は是非連絡ください。
