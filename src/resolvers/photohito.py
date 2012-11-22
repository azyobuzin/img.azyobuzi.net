# -*- coding: utf-8 -*-

import os
import re
import MySQLdb
import urllib2
from sgmllib import SGMLParser
from private_constant import *

class PhotohitoParser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.uri_prefix = None

    def do_meta(self, attributes):
        dic = dict(attributes)
        if "property" in dic and dic["property"] == "og:image":
            content = dic["content"]
            self.uri_prefix = content[: - len("_s.jpg")]

class photohito:
    def __str__(self):
        return "PHOTOHITO"

    regexStr = "^https?://(?:www\\.)?photohito\\.com/photo/(\\d+)/?(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        id = match.group(1)

        if self.last is not None and str(self.last[0]) == id:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM photohito WHERE id = %s", id)
        sqlResult = c.fetchone()

        if sqlResult is None:
            httpRes = urllib2.urlopen("http://photohito.com/photo/%s/" % id)
            html = httpRes.read().decode("utf-8")

            if "<div id=\"error_title\">" in html:
                return None

            parser = PhotohitoParser()
            parser.feed(html)
            parser.close()

            prefix = parser.uri_prefix

            c.execute("INSERT INTO photohito VALUES (%s, %s)", (id, prefix))
            db.commit()

            self.last = (id, prefix)
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)
        return data[1] + "_o.jpg" if data is not None else None

    def getLargeSize(self, match):
        data = self.getUriData(match)
        return data[1] + "_m.jpg" if data is not None else None

    def getThumbnail(self, match):
        data = self.getUriData(match)
        return data[1] + "_s.jpg" if data is not None else None
