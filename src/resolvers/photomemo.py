# -*- coding: utf-8 -*-

from resolvers import *

class Photomemo(OpenGraphResolver):
    @property
    def service_name(self):
        return "Photomemo"

    @property
    def regex_str(self):
        return r"^https?://(?:www\.)?photomemo\.jp/\w+/(\d+)/?(?:\?.*)?$"

    def get_parameters(self, match):
        return match.group(1)

    def _work(self, param, cursor):
        table = "photomemo"
        columns = ["id", "filename"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param})
        if result:
            return result[0]

        try:
            uri = self.read_og("http://photomemo.jp/p/" + param)
        except urllib2.HTTPError as e:
            if e.code == 404:
                raise PictureNotFoundError()
            else:
                raise e

        filename = uri[len("http://photomemo.jp/show_image/panel/"):]

        self.insert_all(cursor, table, (param, filename))
        return filename

    def get_full(self, match):
        return "http://photomemo.jp/show_image/show_image/" + self.work(match)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return "http://photomemo.jp/show_image/panel/" + self.work(match)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
