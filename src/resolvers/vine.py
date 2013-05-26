# -*- coding: utf-8 -*-

from resolvers import *

class Vine(OpenGraphResolver):
    @property
    def service_name(self):
        return "Vine"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?vine\.co/v/(\w+)(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "vine"
        columns = ["id", "image"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            uri = self.read_og("http://vine.co/v/" + param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        self.insert_all(cursor, table, (param, uri))
        return uri

    def get_full(self, match):
        return self.get_full_https(match).replace("https://", "http://", 1)

    def get_full_https(self, match):
        return self.work(match)

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return self.get_full_https(match)

    def get_thumb(self, match):
        return self.get_full(match)

    def get_thumb_https(self, match):
        return self.get_full_https(match)

    def get_video(self, match):
        return self.get_video_https(match).replace("https://", "http://", 1)

    def get_video_https(self, match):
        uri = self.get_full_https(match).replace("/thumbs/", "/videos/", 1).replace(".mp4.jpg", ".mp4", 1)
        return uri[:uri.index("?")]
