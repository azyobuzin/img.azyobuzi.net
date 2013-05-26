# img.azyobuzi.net #
[http://img.azyobuzi.net/](http://img.azyobuzi.net/) のソースコードです。

不具合報告など Issue または Twitter @azyobuzin に投げていただけるとうれしいです。

## ブランチについて ##
master は本番サーバーで稼働中のもの、 dev は機能追加・バグ修正のテスト中または公開待機中のものです。

## ミラーサイトの作り方 ##
- Apache ・ mod_wsgi ・ Python 2.7（2.6 でも動くかも）・ MySQL 5 が実行できるサーバーを用意する
- ライブラリをインストールする
    - [python-oauth2](http://pypi.python.org/pypi/oauth2)
    - [MySQL-python](http://pypi.python.org/pypi/MySQL-python)
    - [Werkzeug](http://pypi.python.org/pypi/Werkzeug)
- src の中身を放り込む
- mirroring/private_constant.py を resolvers ディレクトリに突っ込み、環境に合わせて db_host, db_port, db_name, db_user, db_password を書き換える
- dbsnapshot_struct.sql を MySQL で実行
- mod_wsgi を設定（WSGIScriptAlias だけで大丈夫です）

これで多分動くと思います。

本サーバーが負担高めなので、多くのトラフィックが発生すると予想される場合は、ミラーサイトにご協力お願いします。
