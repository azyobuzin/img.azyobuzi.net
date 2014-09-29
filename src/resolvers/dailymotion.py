# -*- coding: utf-8 -*-

import json
import urllib2

from resolvers import *

class Dailymotion(StoringResolver):
    @property
    def service_name(self):
        return "Dailymotion"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?dailymotion\.com/video/([^/\?]+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "dailymotion"
        columns = ["id", "thumbnail_large", "thumbnail_medium", "thumbnail"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return dict(zip(columns[1:], result))

        try:
            response = urllib2.urlopen("https://api.dailymotion.com/video/%s?fields=thumbnail_large_url,thumbnail_medium_url,thumbnail_url" % urllib2.quote(param))
        except urllib2.HTTPError as e:
            if e.code == 400:
                raise PictureNotFoundError()
            else:
                raise e

        j = json.load(response)

        thumbnail_large = j["thumbnail_large_url"]
        thumbnail_medium = j["thumbnail_medium_url"]
        thumbnail = j["thumbnail_url"]

        self.insert_all(cursor, table, (param, thumbnail_large, thumbnail_medium, thumbnail))
        return dict(zip(columns[1:], (thumbnail_large, thumbnail_medium, thumbnail)))

    def get_full(self, match):
        return self.work(match)["thumbnail"].replace("https://", "http://", 1)

    def get_full_https(self, match):
        return self.work(match)["thumbnail"].replace("http://", "https://", 1)

    def get_large(self, match):
        return self.work(match)["thumbnail_large"].replace("https://", "http://", 1)

    def get_large_https(self, match):
        return self.work(match)["thumbnail_large"].replace("http://", "https://", 1)

    def get_thumb(self, match):
        return self.work(match)["thumbnail_medium"].replace("https://", "http://", 1)

    def get_thumb_https(self, match):
        return self.work(match)["thumbnail_medium"].replace("http://", "https://", 1)

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
