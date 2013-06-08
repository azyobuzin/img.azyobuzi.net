# -*- coding: utf-8 -*-

import urllib2

from resolvers import *

class Imepic(TwitterCardResolver):
    @property
    def service_name(self):
        return u"イメピク"

    @property
    def regex_str(self):
        return r"^http://(?:www\.)?imepic.jp/(\d+)/(\d+)(?:\?.*)?$"

    def get_parameters(self, match):
        return {"date": match.group(1), "id": match.group(2)}

    def _work(self, param, cursor):
        table = "imepic"
        columns = ["date", "id", "image"]
        result = self.select_one(cursor, table, columns[2:], {columns[0]: param["date"], columns[1]: param["id"]})
        if result:
            return result[0]

        try:
            image = self.read_twitter_card("http://imepic.jp/%(date)s/%(id)s" % param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        self.insert_all(cursor, table, (param["date"], param["id"], image))
        return image

    def get_full(self, match):
        return self.work(match)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return self.work(match).replace("/image/", "/thumb/", 1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
