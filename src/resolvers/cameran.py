# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from sgmllib import SGMLParser
from private_constant import *

class CameranParser(SGMLParser):
    def __init__(self):
        SGMLParser.__init__(self)
        self.uri = None

    def do_meta(self, attributes):
        dic = dict(attributes)
        if "property" in dic and dic["property"] == "og:image":
            self.uri = dic["content"]

class cameran:
    def __str__(self):
        return "cameran"

    regexStr = r"^http://cameran.in/posts/get/v1/(\w+)(?:\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        id = match.group(1)

        if self.last is not None and self.last[0] == id:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM cameran WHERE id = %s", id)
        sqlResult = c.fetchone()

        if sqlResult is None:
            reqUri = "http://cameran.in/posts/get/v1/" + id
            httpRes = urllib2.urlopen(reqUri)

            if httpRes.geturl() != reqUri:
                return None

            html = httpRes.read().decode("utf-8")

            parser = CameranParser()
            parser.feed(html)
            parser.close()

            image = parser.uri

            c.execute("INSERT INTO cameran VALUES (%s, %s)", (id, image))
            db.commit()

            self.last = (id, image)
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)
        return data[1] if data is not None else None

    def getLargeSize(self, match):
        data = self.getUriData(match)
        return data[1] if data is not None else None

    def getThumbnail(self, match):
        data = self.getUriData(match)
        return data[1] if data is not None else None
