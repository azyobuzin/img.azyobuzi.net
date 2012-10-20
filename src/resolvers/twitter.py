# -*- coding: utf-8 -*-

import oauth2
import re
import MySQLdb
import json
from private_constant import *

class twitter:
    def __str__(self):
        return "Twitter"

    regexStr = "^(?:(https?://(?:\\w+\\.twimg\\.com/media|p\\.twimg\\.com)/[\\w\\-]+\\.\\w+)(?::\\w+)?|https?://(?:www\\.)?twitter\\.com/(?:#!/)?\\w+/status(?:es)?/(\\d+)/photo/1(?:/(?:\\w+/?)?)?)(?:\\?.*)?$"
    regex = re.compile(regexStr, re.IGNORECASE)

    last = None

    def getUriData(self, match):
        media = match.group(1)

        if media is not None:
            return [None, media]

        id = match.group(2)

        if self.last is not None and str(self.last[0]) == id:
            return self.last

        db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
        c = db.cursor()

        c.execute("SELECT * FROM twitter WHERE id = " + db.literal(id))
        sqlResult = c.fetchone()

        if sqlResult is None:
            resp, content = twitterOAuthClient.request("https://api.twitter.com/1.1/statuses/show.json?include_entities=true&id=" + id, "GET")

            if resp["status"] == "404":
                return None
            elif resp["status"] != "200":
                raise Exception("Twitter returned " + resp["status"])

            entities = json.loads(content)["entities"]

            if "media" not in entities:
                return None

            media = entities["media"][0]["media_url"]

            c.execute("INSERT INTO twitter VALUES (%s, %s)"
                % (db.literal(id), db.literal(media))
            )

            db.commit()

            self.last = [id, media]
        else:
            self.last = sqlResult

        return self.last

    def getFullSize(self, match):
        data = self.getUriData(match)
        return (data[1] + ":large") if data is not None else None

    def getLargeSize(self, match):
        data = self.getUriData(match)
        return data[1] if data is not None else None

    def getThumbnail(self, match):
        data = self.getUriData(match)
        return (data[1] + ":thumb") if data is not None else None
