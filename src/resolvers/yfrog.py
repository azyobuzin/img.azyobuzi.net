# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import xml.etree.ElementTree as ET
from private_constant import *

class yfrog:
    def __str__(self):
        return "yfrog"

    regexStr = "^https?://(?:www\\.|twitter\\.)?yfrog\\.com/(\\w+)(?::\\w+)?(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        hash = match.group(1)

        if self.last is not None and self.last[0] == hash:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM yfrog WHERE hash = %s", hash)
        sqlResult = c.fetchone()

        if sqlResult is None:
            httpRes = None

            try:
                httpRes = urllib2.urlopen("http://yfrog.com/api/xmlInfo?path=" + hash)
            except:
                return None

            root = ET.fromstring(httpRes.read())
            tag = root.tag
            ns = tag[: tag.index("}") + 1]

            image = root.find(ns + "links").find(ns + "image_link").text

            c.execute("INSERT INTO yfrog VALUES (%s, %s)", (hash, image))
            db.commit()

            self.last = (hash, image)
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)
        return data[1] if data is not None else None

    def getLargeSize(self, match):
        return "http://yfrog.com/%s:iphone" % match.group(1)

    def getThumbnail(self, match):
        return "http://yfrog.com/%s:small" % match.group(1)
