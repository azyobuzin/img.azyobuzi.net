# -*- coding: utf-8 -*-

import abc
import re
from sgmllib import SGMLParser
import urllib2

import MySQLdb

from private_constant import *

class Resolver(object):
    __metaclass__ = abc.ABCMeta

    def __init__(self):
        self._regex = None

    @abc.abstractproperty
    def service_name(self):
        return None

    @abc.abstractproperty
    def regex_str(self):
        return None

    @property
    def regex(self):
        if self._regex is None:
            self._regex = re.compile(self.regex_str, re.IGNORECASE)
        return self._regex

    @abc.abstractmethod
    def get_full(self, match):
        return None

    @abc.abstractmethod
    def get_full_https(self, match):
        return None

    @abc.abstractmethod
    def get_large(self, match):
        return None

    @abc.abstractmethod
    def get_large_https(self, match):
        return None

    @abc.abstractmethod
    def get_thumb(self, match):
        return None

    @abc.abstractmethod
    def get_thumb_https(self, match):
        return None

    @abc.abstractmethod
    def get_video(self, match):
        return None

    @abc.abstractmethod
    def get_video_https(self, match):
        return None

class StoringResolver(Resolver):
    "DB に保存するタイプのリゾルバー"

    def __init__(self):
        super(StoringResolver, self).__init__()
        self._cached_param = None
        self._cached_result = None

    def _connect_db(self):
        return MySQLdb.connect(user=db_user, passwd=db_password, db=db_name, charset="utf8")

    @abc.abstractmethod
    def get_parameters(self, match):
        "match の結果から DB 参照時に使うパラメータを取得する"
        pass

    @abc.abstractmethod
    def _work(self, param, cursor):
        pass

    def work(self, match):
        param = self.get_parameters(match)
        if param == self._cached_param:
            return self._cached_result
        else:
            with self._connect_db() as cursor: #Connection.__enter__() で Cursor が返される
                result = self._work(param, cursor)
                self._cached_param = param
                self._cached_result = result
                return result

    @staticmethod
    def select_one(cursor, table, columns, conditions):
        columns = tuple(columns)
        conditions = dict(conditions)
        query = "SELECT %s FROM %s WHERE %s" \
            % (", ".join(columns), table, " AND ".join("%s = %%s" % c for c in conditions.iterkeys()))
        cursor.execute(query, tuple(conditions.itervalues()))
        return cursor.fetchone()

    @staticmethod
    def insert_all(cursor, table, values):
        "カラムを指定しないで INSERT INTO"
        values = tuple(values)
        query = "INSERT INTO %s VALUES(%s)" % (table, ", ".join(["%s"] * len(values)))
        cursor.execute(query, values)

class OpenGraphResolver(StoringResolver):
    class OpenGraphParser(SGMLParser):
        def __init__(self):
            SGMLParser.__init__(self)
            self.uri = None

        def do_meta(self, attributes):
            dic = dict(attributes)
            if dic.get("property") == "og:image":
                self.uri = dic["content"]

    @staticmethod
    def read_og(uri, check=None, encoding="utf-8"):
        u"""OpenGraph を読み込みに行きます。

        uri: 取得しに行く URI
        check: 正しいレスポンスかどうかを確認するラムダ式

        成功時には og:image の内容を、 check が False を返した場合には False を、 og:image が見つからなかった場合には None を返します。
        """
        #あとでわからなくなりそうなので書いておいた

        response = urllib2.urlopen(uri)

        if check is not None and not check(response):
            return False

        parser = OpenGraphResolver.OpenGraphParser()
        parser.feed(response.read().decode(encoding))
        parser.close()

        return parser.uri

class PictureNotFoundError(Exception):
    pass
