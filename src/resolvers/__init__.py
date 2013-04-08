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

        if check and not check(response):
            return False

        parser = OpenGraphResolver.OpenGraphParser()
        parser.feed(response.read().decode(encoding))
        parser.close()

        return parser.uri

class PictureNotFoundError(Exception):
    pass

class Request2(urllib2.Request):
    def __init__(self, url, data=None, headers={}, origin_req_host=None,
                 unverifiable=False, method=None):
        urllib2.Request.__init__(self, url, data, headers, origin_req_host, unverifiable)
        self._method = method

    def get_method(self):
        if self._method:
            return self._method
        else:
            if self.has_data():
                return "POST"
            else:
                return "GET"

class HTTPRedirectHandler2(urllib2.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        """Return a Request or None in response to a redirect.

        This is called by the http_error_30x methods when a
        redirection response is received.  If a redirection should
        take place, return a new Request to allow http_error_30x to
        perform the redirect.  Otherwise, raise HTTPError if no-one
        else should try to handle this url.  Return None if you can't
        but another Handler might.
        """
        m = req.get_method()
        if (code in (301, 302, 303, 307) and m in ("GET", "HEAD")
            or code in (301, 302, 303) and m == "POST"):
            # Strictly (according to RFC 2616), 301 or 302 in response
            # to a POST MUST NOT cause a redirection without confirmation
            # from the user (of urllib2, in this case).  In practice,
            # essentially all clients do redirect in this case, so we
            # do the same.
            # be conciliant with URIs containing a space
            newurl = newurl.replace(' ', '%20')
            newheaders = dict((k,v) for k,v in req.headers.items()
                              if k.lower() not in ("content-length", "content-type")
                             )
            return Request2(newurl,
                           headers=newheaders,
                           origin_req_host=req.get_origin_req_host(),
                           unverifiable=True,
                           method="GET" if code == 303 else m)
        else:
            raise urllib2.HTTPError(req.get_full_url(), code, msg, headers, fp)

class DontRedirectHandler(urllib2.HTTPRedirectHandler):
    http_error_301 = http_error_302 = http_error_303 =\
        http_error_307 = lambda self, req, fp, code, msg, headers: fp

urllib2.install_opener(HTTPRedirectHandler2)
