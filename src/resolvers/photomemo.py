# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from sgmllib import SGMLParser
from private_constant import *

class PhotomemoParser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.filename = None

    def do_meta(self, attributes):
        dic = dict(attributes)
        if "property" in dic and dic["property"] == "og:image":
            content = dic["content"]
            self.filename = content[len("http://photomemo.jp/show_image/panel/") :]

class photomemo:
    def __str__(self):
        return "Photomemo"

    regexStr = "^https?://(?:www\\.)?photomemo\\.jp/\\w+/(\\d+)/?(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        id = match.group(1)

        if self.last is not None and str(self.last[0]) == id:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM photomemo WHERE id = %s", id)
        sqlResult = c.fetchone()

        if sqlResult is None:
            httpRes = None
            try:
                httpRes = urllib2.urlopen("http://photomemo.jp/p/" + id)
            except:
                return None

            html = httpRes.read().decode("utf-8")

            parser = PhotomemoParser()
            parser.feed(html)
            parser.close()

            filename = parser.filename

            c.execute("INSERT INTO photomemo VALUES (%s, %s)", (id, filename))
            db.commit()

            self.last = (id, filename)
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)
        return "http://photomemo.jp/show_image/show_image/" + data[1] if data is not None else None

    def getLargeSize(self, match):
        data = self.getUriData(match)
        return "http://photomemo.jp/show_image/show_image/" + data[1] if data is not None else None

    def getThumbnail(self, match):
        data = self.getUriData(match)
        return "http://photomemo.jp/show_image/panel/" + data[1] if data is not None else None
