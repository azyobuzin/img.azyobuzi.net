# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from sgmllib import SGMLParser
from private_constant import *

class My365Parser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.uri = None

    def do_meta(self, attributes):
        dic = dict(attributes)
        if "property" in dic and dic["property"] == "og:image":
            self.uri = dic["content"]

class my365:
    def __str__(self):
        return "My365"

    regexStr = r"^https?://(?:www\.)?my365\.in/([\w\-]+)/p/(\d+)/?(?:\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        username = match.group(1)
        id = match.group(2)

        if self.last is not None and self.last[0] == username and str(self.last[1]) == id:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM my365 WHERE username = %s AND id = %s", (username, id))
        sqlResult = c.fetchone()

        if sqlResult is None:
            reqUri = "http://my365.in/%s/p/%s" % (username, id)

            try:
                httpRes = urllib2.urlopen(reqUri)
            except:
                return None

            if httpRes.geturl() != reqUri: #存在しないときはユーザーページへリダイレクトされる
                return None

            html = httpRes.read().decode("utf-8")

            parser = My365Parser()
            parser.feed(html)
            parser.close()

            image = parser.uri

            c.execute("INSERT INTO my365 VALUES (%s, %s, %s)", (username, id, image))
            db.commit()

            self.last = (username, id, image)
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)
        return data[2] if data is not None else None

    def getLargeSize(self, match):
        data = self.getUriData(match)
        return data[2] if data is not None else None

    def getThumbnail(self, match):
        data = self.getUriData(match)
        return data[2] if data is not None else None
