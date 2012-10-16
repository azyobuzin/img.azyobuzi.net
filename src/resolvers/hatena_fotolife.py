# -*- coding: utf-8 -*-

import os
import re
import MySQLdb
import urllib2
import urlparse
from sgmllib import SGMLParser
from private_constant import *

class HatenaFotolifeParser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.foto_src = None
        self.original_link = None

    def do_img(self, attributes):
        dic = dict(attributes)
        if "id" in dic and dic["id"].startswith("foto-for-html-tag-"):
            self.foto_src = dic["src"]

    def start_a(self, attributes):
        dic = dict(attributes)
        if "href" in dic and dic["href"].find("_original.") != -1:
            self.original_link = dic["href"]

class hatenaFotolife:
    def __str__(self):
        return u"はてなフォトライフ"

    regexStr = "^http://f\\.hatena\\.ne\\.jp/(\\w+)/(\\d+)(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        username = match.group(1)
        id = match.group(2)

        if self.last is not None and self.last[0] == username and str(self.last[1]) == id:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM hatena_fotolife WHERE username = %s AND id = %s"
            % (db.literal(username), db.literal(id))
        )

        sqlResult = c.fetchone()

        if sqlResult is None:
            reqUri = "http://f.hatena.ne.jp/%s/%s" % (username, id)
            httpRes = None

            try:
                httpRes = urllib2.urlopen(reqUri)
            except:
                return None

            if httpRes.geturl() != reqUri: #リダイレクトされたと判断
                return None

            html = httpRes.read().decode("utf-8")

            if html.find("<img src=\"\" alt=\"\" title=\"\" width=\"\" height=\"\" class=\"foto\" style=\"\" />") != -1:
                return None

            originalSize = None
            largeSize = None
            thumbnail = None

            parser = HatenaFotolifeParser()
            parser.feed(html)
            parser.close()

            largeSize = parser.foto_src

            if html.find("<object data=\"/tools/flvplayer.swf\" type=\"application/x-shockwave-flash\"") == -1:
                #画像
                if parser.original_link is not None:
                    originalSize = parser.original_link
                else:
                    originalSize = largeSize
            else:
                #動画
                originalSize = "http://cdn-ak.f.st-hatena.com/images/fotolife/%s/%s/%s/%s.flv" % (username[0], username, id[0:8], id)

            thumbnail = "http://cdn-ak.f.st-hatena.com/images/fotolife/%s/%s/%s/%s_120.jpg" % (username[0], username, id[0:8], id)

            c.execute("INSERT INTO hatena_fotolife VALUES (%s, %s, %s, %s, %s)"
                % (db.literal(username), db.literal(id), db.literal(originalSize), db.literal(largeSize), db.literal(thumbnail))
            )

            db.commit()

            self.last = [username, id, originalSize, largeSize, thumbnail]
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)

        if data is None:
            return None

        ret = data[2]

        query = dict(urlparse.parse_qsl(os.environ.get("QUERY_STRING", "")))
        if "movie" in query and query["movie"] == "true":
            return ret
        else:
            return ret.replace(".flv", ".jpg")

    def getLargeSize(self, match):
        data = self.getUriData(match)
        return data[3] if data is not None else None

    def getThumbnail(self, match):
        data = self.getUriData(match)
        return data[4] if data is not None else None
