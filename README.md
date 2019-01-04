# img.azyobuzi.net #
[http://img.azyobuzi.net/](http://img.azyobuzi.net/) のソースコードです。

不具合報告など Issue または Twitter @azyobuzin に投げていただけるとうれしいです。

## ブランチについて ##
master は本番サーバーで稼働中のもの、 dev は機能追加・バグ修正のテスト中または公開待機中のものです。

## 動かしかた ##
- Python 2.7（2.6 でも動くかも）・ MySQL 5 が実行できるサーバーを用意する
- ライブラリをインストールする
    - [python-oauth2](http://pypi.python.org/pypi/oauth2)
    - [MySQL-python](http://pypi.python.org/pypi/MySQL-python)
    - [Werkzeug](http://pypi.python.org/pypi/Werkzeug)
- src の中身を放り込む
- 環境変数 IMGAZYOBUZI_DB_HOST, IMGAZYOBUZI_DB_POR, IMGAZYOBUZI_DB_NAME ,IMGAZYOBUZI_DB_USER, IMGAZYOBUZI_DB_PASSWORD をいい感じに設定する
- dbsnapshot_struct.sql を MySQL で実行
- gunicorn で動かす

これで多分動くと思います。
