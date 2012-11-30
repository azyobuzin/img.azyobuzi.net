# -*- coding: utf-8 -*-

import re
import MySQLdb
import httplib
from private_constant import *

class dropbox:
    def __str__(self):
        return "Dropbox"

    regexStr = "^https?://(?:(?:www\\.|dl\\.)?dropbox\\.com/s/(\\w+)/([\\w\\-\\.%]+\\.(?:jpeg?|jpg|png|gif|bmp|dib|tiff?))|(?:www\\.)?db\\.tt/(\\w+))/?(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUri(self, match):
        shorten = match.group(3)
        if shorten is not None:
            if self.last is not None and self.last[0] == shorten:
                exMatch = self.regex.match(self.last[1])
                return self.getUri(exMatch) if exMatch is not None else None

            db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
            c = db.cursor()

            c.execute("SELECT * FROM dropbox WHERE shorten = %s", shorten)
            sqlResult = c.fetchone()

            if sqlResult is None:
                conn = httplib.HTTPConnection("db.tt")
                conn.request("HEAD", "/" + shorten)
                res = conn.getresponse()
                uri = res.getheader("location")
                res.close()
                c.execute("INSERT INTO dropbox VALUES (%s, %s)", (shorten, uri))
                db.commit()
                self.last = (shorten, uri)
                exMatch = self.regex.match(uri)
                return self.getUri(exMatch) if exMatch is not None else None
            else:
                self.last = sqlResult
                exMatch = self.regex.match(sqlResult[1])
                return self.getUri(exMatch) if exMatch is not None else None

        token = match.group(1)
        filename = match.group(2)
        return "https://dl.dropbox.com/s/%s/%s" % (token, filename)

    def getFullSize(self, match):
        return self.getUri(match)

    def getLargeSize(self, match):
        return self.getUri(match)

    def getThumbnail(self, match):
        return self.getUri(match)
