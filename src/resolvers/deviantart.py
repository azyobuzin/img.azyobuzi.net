# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *

class DeviantArt(StoringResolver):
    @property
    def service_name(self):
        return "deviantART"

    @property
    def regex_str(self):
        return r"^https?://([\w\-]+)\.deviantart\.com/art/([\w\-]+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return {"username": match.group(1), "id": match.group(2)}

    def _work(self, param, cursor):
        table = "deviantart"
        columns = ["username", "id", "full", "thumbnail", "thumbnail150"]
        result = self.select_one(cursor, table, columns[2:], {columns[0]: param["username"], columns[1]: param["id"]})
        if result:
            return dict(zip(columns[2:], result))

        try:
            response = urllib2.urlopen(
                "http://backend.deviantart.com/oembed?url="
                + urllib2.quote("http://%s.deviantart.com/art/%s" % (param["username"], param["id"]))
            )
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        j = json.load(response)

        full = j["url"]
        thumbnail = j["thumbnail_url"]
        thumbnail150 = j["thumbnail_url_150"]

        self.insert_all(cursor, table, (param["username"], param["id"], full, thumbnail, thumbnail150))
        return dict(zip(columns[2:], (full, thumbnail, thumbnail150)))

    def get_full(self, match):
        return self.work(match)["full"]

    def get_full_https(self, match):
        return self.get_full(match).replace("http://", "https://", 1)

    def get_large(self, match):
        return self.work(match)["thumbnail"]

    def get_large_https(self, match):
        return self.get_large(match).replace("http://", "https://", 1)

    def get_thumb(self, match):
        return self.work(match)["thumbnail150"]

    def get_thumb_https(self, match):
        return self.get_thumb(match).replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
