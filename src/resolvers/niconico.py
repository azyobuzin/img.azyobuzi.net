# -*- coding: utf-8 -*-

import urllib2
from xml.etree import ElementTree

from resolvers import *

class Niconico(StoringResolver):
    @property
    def service_name(self):
        return u"ニコニコ動画"

    @property
    def regex_str(self):
        return r"^http://(?:(?:www\.)?nicovideo\.jp/watch|nico\.(?:ms|sc))/([sn]m\d+)?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "niconico"
        columns = ["id", "thumbnail"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        response = urllib2.urlopen("http://ext.nicovideo.jp/api/getthumbinfo/" + param)

        root = ElementTree.fromstring(response.read())
        thumb = root.find("thumb")

        if thumb is None:
            raise PictureNotFoundError()

        thumbnail = thumb.find("thumbnail_url").text

        self.insert_all(cursor, table, (param, thumbnail))
        return thumbnail

    def get_full(self, match):
        result = self.work(match)
        return result + (".L" if int(match.group(1)[2:]) >= 16371850 else "")

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return self.work(match)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
